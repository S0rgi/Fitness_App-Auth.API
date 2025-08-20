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

namespace Gainly_Auth_API.Service
{
    public class AuthService : IAuthService
    {
        private readonly AuthDbContext _context;
        private readonly IUserRepository _users;
        private readonly IRefreshTokenRepository _refreshTokens;
        private readonly INotificationPublisher _publisher;
        private readonly IConfiguration _config;
        private readonly IUserAuthenticationService _tokenGen;
        private readonly ITokenService _tokenService;
        private readonly IUsernameGenerator _usernameGenerator;

        public AuthService(AuthDbContext context, INotificationPublisher publisher, IConfiguration config, IUserAuthenticationService tokenGen, ITokenService tokenService, IUsernameGenerator usernameGenerator, IUserRepository users, IRefreshTokenRepository refreshTokens)
        {
            _context = context;
            _publisher = publisher;
            _config = config;
            _tokenGen = tokenGen;
            _tokenService = tokenService;
            _usernameGenerator = usernameGenerator;
            _users = users;
            _refreshTokens = refreshTokens;
        }

        public async Task<AuthResult> RegisterAsync(RegisterDto request, CancellationToken cancellationToken = default)
        {
            if (await _users.ExistsByEmailAsync(request.Email, cancellationToken))
                return new AuthResult(false, "Email is already registered", null);

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
            await _context.SaveChangesAsync(cancellationToken);
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
                return new EmailCodeResult(false, null);
            var code = new Random().Next(10000, 99999);
            var notification = new NotificationMessage
            {
                Type = "code",
                Action = code.ToString(),
                SenderName = "none",
                RecipientEmail = email
            };
            _publisher.PublishAsync(JsonSerializer.Serialize(notification), "code");
            return new EmailCodeResult(true, code);
        }
        public async Task<AuthResult> GoogleLoginAsync(string GoogleIdToken, CancellationToken cancellationToken = default)
        {
            Payload payload;
            try {  payload = await ValidateAsync(GoogleIdToken); }
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

        public async Task<AuthResult> TGLoginAsync(string tgLogin, CancellationToken cancellationToken = default)
        {
            var user = await _users.FindByTgLoginAsync(tgLogin, cancellationToken);
            if (user == null)
            {
                user = new User
                {
                    Username = await _usernameGenerator.GenerateAsync(tgLogin),
                    Email= string.Empty,
                    PasswordHash = string.Empty,
                    RegistrationDate = DateTime.UtcNow,
                    TGUsername = tgLogin
                };

                await _users.AddAsync(user, cancellationToken);
                await _users.SaveChangesAsync(cancellationToken);

            }
            var tokens = await _tokenGen.GenerateTokensAsync(user);
            return new AuthResult(true, null, tokens);
            
        }

    }
}



