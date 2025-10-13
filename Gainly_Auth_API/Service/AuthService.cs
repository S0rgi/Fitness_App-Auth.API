using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using Gainly_Auth_API.Data;
using Gainly_Auth_API.Models;
using Gainly_Auth_API.Dtos;
using Gainly_Auth_API.Interfaces;
using System.Text.Json;
using static Google.Apis.Auth.GoogleJsonWebSignature;
using Google.Apis.Auth;
using System.Web;
using System.Text.Json.Serialization;

namespace Gainly_Auth_API.Service
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _users;
        private readonly IRefreshTokenRepository _refreshTokens;
        private readonly INotificationPublisher _publisher;
        private readonly IUserAuthenticationService _tokenGen;
        private readonly ITokenService _tokenService;
        private readonly IUsernameGenerator _usernameGenerator;
        private readonly ITelegramAuthValidator _telegramAuthValidator;

        public AuthService(INotificationPublisher publisher, IUserAuthenticationService tokenGen, ITokenService tokenService, IUsernameGenerator usernameGenerator, IUserRepository users, IRefreshTokenRepository refreshTokens, ITelegramAuthValidator telegramAuthValidator)
        {
            _publisher = publisher;
            _tokenGen = tokenGen;
            _tokenService = tokenService;
            _usernameGenerator = usernameGenerator;
            _users = users;
            _refreshTokens = refreshTokens;
            _telegramAuthValidator = telegramAuthValidator;
        }

        public async Task<AuthResult> RegisterAsync(RegisterDto request, CancellationToken cancellationToken = default)
        {
            if (await _users.ExistsByEmailAsync(request.Email, cancellationToken))
                return new AuthResult(false, "Email is already registered", null);

            // Проверка подтверждения email
            var confirmation = await _users.GetLastEmailConfirmationAsync(request.Email, cancellationToken);
            if (confirmation == null || !confirmation.IsConfirmed)
                return new AuthResult(false, "Email not confirmed", null);

            var user = new User
            {
                Email = request.Email,
                Username = await _usernameGenerator.GenerateAsync(request.Email),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                RegistrationDate = DateTime.UtcNow
            };

            await _users.AddAsync(user, cancellationToken);
            await _users.SaveChangesAsync(cancellationToken);

            var tokens = await _tokenGen.GenerateTokensAsync(user);
            return new AuthResult(true, null, tokens);
        }

        public async Task<AuthResult> LoginAsync(LoginDto request, CancellationToken cancellationToken = default)
        {
            var user = await _users.FindByEmailAsync(request.Email, cancellationToken);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return new AuthResult(false, "Invalid credentials", null);

            var tokens = await _tokenGen.GenerateTokensAsync(user);
            return new AuthResult(true, null, tokens);
        }

        public async Task<bool> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            var token = await _refreshTokens.FindByTokenAsync(refreshToken, cancellationToken);
            if (token == null)
                return false;

            token.IsRevoked = true;
            await _refreshTokens.SaveChangesAsync(cancellationToken);
            return true;
        }
        public async Task<TokenPair> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(refreshToken))
                return null;

            var oldToken = await _refreshTokens.FindByTokenAsync(refreshToken, cancellationToken);

            if (oldToken == null || oldToken.IsRevoked || oldToken.ExpiresAt < DateTime.UtcNow)
                return null;

            var user = await _users.FindByIdAsync(oldToken.UserId, cancellationToken);
            if (user == null)
                return null;

            // Отзываем старый токен
            oldToken.IsRevoked = true;

            // Генерируем новую пару токенов
            var tokenPair = await _tokenGen.GenerateTokensAsync(user);

            await _refreshTokens.SaveChangesAsync(cancellationToken);

            return tokenPair;
        }

        public Task<Interfaces.TokenValidationResult> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            try
            {
                _ = _tokenService.ValidateAccessToken(token);
                return Task.FromResult(new Interfaces.TokenValidationResult(true, null));
            }
            catch (Exception ex)
            {
                return Task.FromResult(new Interfaces.TokenValidationResult(false, ex.Message));
            }
        }

        public async Task<EmailCodeResult> SendEmailCodeAsync(string email, CancellationToken cancellationToken = default)
        {
            if (await _users.ExistsByEmailAsync(email, cancellationToken))
                return new EmailCodeResult(false, "email busy");
            var code = new Random().Next(10000, 99999).ToString();

            // Сохраняем код в БД
            var confirmation = new EmailConfirmation
            {
                Id = Guid.NewGuid(),
                Email = email,
                Code = code,
                Expiration = DateTime.UtcNow.AddMinutes(10),
                IsConfirmed = false,
                CreatedAt = DateTime.UtcNow
            };
            await _users.AddEmailConfirmationAsync(confirmation, cancellationToken);
            await _users.SaveChangesAsync(cancellationToken);

            var notification = new NotificationMessage
            {
                Type = "code",
                Action = code,
                SenderName = "none",
                RecipientEmail = email
            };
            _publisher.PublishAsync(JsonSerializer.Serialize(notification), "code");
            return new EmailCodeResult(true, null);
        }
        public async Task<AuthResult> GoogleLoginAsync(string GoogleIdToken, CancellationToken cancellationToken = default)
        {
            Payload payload;
            try { payload = await ValidateAsync(GoogleIdToken); }
            catch (Exception ex) { return new AuthResult(false, ex.Message, null); }
            if (payload == null)
            {
                return new AuthResult(false, "Bad token", null);
            }
            var user = await _users.FindByEmailAsync(payload.Email, cancellationToken);
            if (user != null)
            {
                var tokensLog = await _tokenGen.GenerateTokensAsync(user);
                return new AuthResult(true, null, tokensLog);
            }

            user = new User
            {
                Email = payload.Email,
                Username = await _usernameGenerator.GenerateAsync(payload.Email),
                PasswordHash = "google_login",
                RegistrationDate = DateTime.UtcNow
            };
            await _users.AddAsync(user, cancellationToken);
            await _users.SaveChangesAsync(cancellationToken);

            var tokens = await _tokenGen.GenerateTokensAsync(user);
            return new AuthResult(true, null, tokens);
        }

        public async Task<AuthResult> TGLoginRawAsync(
            TelegramInitDataRawDto req,
            CancellationToken cancellationToken = default)
        {
            if (!_telegramAuthValidator.ValidateInitData(req.initDataRaw, cancellationToken))
                return new AuthResult(false, "init data not valid", null);

            var tgUser = ExtractTelegramUser(req.initDataRaw);
                if (tgUser == null)
                    return new AuthResult(false, "user not found in initData", null);
            string tgLogin = tgUser.Username;
            var user = await _users.FindByTgLoginAsync(tgUser.Id.ToString(), cancellationToken);
            if (user == null)
            {
                user = new User
                {
                    Username = await _usernameGenerator.GenerateAsync(tgLogin),
                    Email = string.Empty,
                    PasswordHash = string.Empty,
                    RegistrationDate = DateTime.UtcNow,
                    TGUsername = tgUser.Id.ToString()
                };

                await _users.AddAsync(user, cancellationToken);
                await _users.SaveChangesAsync(cancellationToken);
            }

            var tokens = await _tokenGen.GenerateTokensAsync(user);
            return new AuthResult(true, null, tokens);
        }
        /// <summary>
        /// Достаёт объект user из initDataRaw
        /// </summary>
        private TgUserDto? ExtractTelegramUser(string initDataRaw)
        {
            var query = HttpUtility.ParseQueryString(initDataRaw);
            string userJson = query["user"];
            if (string.IsNullOrEmpty(userJson))
                return null;

            // Декодируем URL-encoded JSON
            userJson = HttpUtility.UrlDecode(userJson);

            // Парсим JSON
            return JsonSerializer.Deserialize<TgUserDto>(userJson);
        }

        public async Task<EmailCodeResult> CheckEmailCodeAsync(CheckEmailDto dto, CancellationToken cancellationToken = default)
        {
            var confirmation = await _users.GetEmailConfirmationAsync(dto.email, dto.code.ToString(), cancellationToken);
            if (confirmation == null || confirmation.Expiration < DateTime.UtcNow)
                return new EmailCodeResult(false, "Invalid or expired code");

            confirmation.IsConfirmed = true;
            await _users.SaveChangesAsync(cancellationToken);
            return new EmailCodeResult(true, null);
        }

        public async Task<AuthResult> TGLoginAsync(TelegramInitDataDto request, CancellationToken cancellationToken)
        {
            _telegramAuthValidator.ValidateInitData(request);
            
            if (!_telegramAuthValidator.ValidateInitData(request))
                return new AuthResult(false, "init data not valid", null);


            string tgLogin = request.Username;
            var user = await _users.FindByTgLoginAsync(request.Id.ToString(), cancellationToken);
            if (user == null)
            {
                user = new User
                {
                    Username = await _usernameGenerator.GenerateAsync(tgLogin),
                    Email = string.Empty,
                    PasswordHash = string.Empty,
                    RegistrationDate = DateTime.UtcNow,
                    TGUsername = request.Id.ToString()
                };

                await _users.AddAsync(user, cancellationToken);
                await _users.SaveChangesAsync(cancellationToken);
            }

            var tokens = await _tokenGen.GenerateTokensAsync(user);
            return new AuthResult(true, null, tokens);

        }

    }
    public class TgUserDto
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("is_bot")]
        public bool IsBot { get; set; }

        [JsonPropertyName("first_name")]
        public string FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public string LastName { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("language_code")]
        public string LanguageCode { get; set; }
    }
}


