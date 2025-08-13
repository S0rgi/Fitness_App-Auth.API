using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using Fitness_App_Auth.API.Data;
using Fitness_App_Auth.API.Models;
using Fitness_App_Auth.API.Interfaces;
using Fitness_App_Auth.API.Models;
using System.Text.Json;

namespace Fitness_App_Auth.API.Service
{
    public class AuthService : IAuthService
    {
        private readonly AuthDbContext _context;
        private readonly INotificationPublisher _publisher;
        private readonly IConfiguration _config;
        private readonly IUserAuthenticationService _tokenGen;
        private readonly ITokenService _tokenService;
        private readonly IUsernameGenerator _usernameGenerator;

        public AuthService(AuthDbContext context, INotificationPublisher publisher, IConfiguration config, IUserAuthenticationService tokenGen, ITokenService tokenService, IUsernameGenerator usernameGenerator)
        {
            _context = context;
            _publisher = publisher;
            _config = config;
            _tokenGen = tokenGen;
            _tokenService = tokenService;
            _usernameGenerator = usernameGenerator;
        }

        public async Task<AuthResult> RegisterAsync(RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return new AuthResult(false, "Email is already registered", null);

            var user = new User
            {
                Email = request.Email,
                Username = await _usernameGenerator.GenerateAsync(request.Email),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var tokens = await _tokenGen.GenerateTokensAsync(user);
            return new AuthResult(true, null, tokens);
        }

        public async Task<AuthResult> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return new AuthResult(false, "Invalid credentials", null);

            var tokens = await _tokenGen.GenerateTokensAsync(user);
            return new AuthResult(true, null, tokens);
        }
        
        public async Task LogoutAsync(string refreshToken)
        {
            var token = await _context.RefreshTokens.FirstOrDefaultAsync(r => r.Token == refreshToken);
            if (token == null)
                throw new Exception("Refresh token not found");

            token.IsRevoked = true;
            await _context.SaveChangesAsync();
    }
    public async Task<TokenPair> RefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
            return null;

        var oldToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == refreshToken);

        if (oldToken == null || oldToken.IsRevoked || oldToken.ExpiresAt < DateTime.UtcNow)
            return null;

        var user = await _context.Users.FindAsync(oldToken.UserId);
        if (user == null)
            return null;

        // Отзываем старый токен
        oldToken.IsRevoked = true;

        // Генерируем новую пару токенов
        var tokenPair = await _tokenGen.GenerateTokensAsync(user);

        await _context.SaveChangesAsync();

        return tokenPair;
    }

        public Task<Interfaces.TokenValidationResult> ValidateTokenAsync(string token)
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

        public async Task<EmailCodeResult> SendEmailCodeAsync(string email)
        {
              if (await _context.Users.AnyAsync(u => u.Email == email))
                return new EmailCodeResult(false,null);
            var code = new Random().Next(10000, 99999);
            var notification = new NotificationMessage
            {
                Type = "code",
                Action=code.ToString(),
                SenderName = "none",
                RecipientEmail = email
            };
            _publisher.PublishAsync(JsonSerializer.Serialize(notification), "code");
            return new EmailCodeResult(true, code);
        }
    }
}
