using Fitness_App_Auth.API.Data;
using Fitness_App_Auth.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Fitness_App_Auth.API.Dtos;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Fitness_App_Auth.API.Interfaces;
namespace Fitness_App_Auth.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthDbContext _context;
        private readonly IConfiguration _config;

        private readonly ITokenService _tokenService;

        public AuthController(AuthDbContext context, IConfiguration config, ITokenService tokenService)
        {
            _context = context;
            _config = config;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            // Валидация
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest("Email уже занят");

            var user = new User
            {
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Username = dto.Username,
                RegistrationDate = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = _tokenService.GenerateJwtToken(user);
            return Ok(new { user.Id, Token = token });
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginDto dto)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Неверный email или пароль");

            // Генерация JWT
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            });

            var accessToken = _tokenService.GenerateAccessToken(identity);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Сохраняем refresh-токен в БД
            await _context.RefreshTokens.AddAsync(new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id.ToString(),
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });
            await _context.SaveChangesAsync();

            // Возвращаем оба токена
            return Ok(new
            {
                accessToken,
                refreshToken
            });

        }
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] string refreshToken)
        {
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == refreshToken);

            if (token == null || token.IsRevoked || token.ExpiresAt < DateTime.UtcNow)
                return Unauthorized("Invalid refresh token");

            var user = await _context.Users.FindAsync(token.UserId);
            if (user == null)
                return Unauthorized("User not found");

            // Генерация нового токена
            var identity = new ClaimsIdentity(new[]
            {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email)
    });

            var newAccessToken = _tokenService.GenerateAccessToken(identity);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            // Обновляем refresh token
            token.IsRevoked = true;
            _context.RefreshTokens.Add(new RefreshToken
            {
                Token = newRefreshToken,
                UserId = user.Id.ToString(),
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                accessToken = newAccessToken,
                refreshToken = newRefreshToken
            });
        }
    }
}
