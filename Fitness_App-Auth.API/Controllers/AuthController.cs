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
using Fitness_App_Auth.API.Service;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
namespace Fitness_App_Auth.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthDbContext _context;
        private readonly IConfiguration _config;

        private readonly ITokenService _tokenService;
        private readonly IAuthService _authService;
        IUsernameGenerator _usernameGenerator;
        public AuthController(AuthDbContext context, IConfiguration config, ITokenService tokenService, IAuthService authService,IUsernameGenerator usernameGenerator)
        {
            _context = context;
            _config = config;
            _tokenService = tokenService;
            _authService = authService;
            _usernameGenerator = usernameGenerator;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest("Email уже занят");

            var username = await _usernameGenerator.GenerateAsync(dto.Email);

            var user = new User
            {
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Username = username,
                RegistrationDate = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var (accessToken, refreshToken) = await _authService.GenerateTokensAsync(user);
            return Ok(new { accessToken, refreshToken,user.Id });
        }


        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Неверный email или пароль");

            var (accessToken, refreshToken) = await _authService.GenerateTokensAsync(user);
            return Ok(new { accessToken, refreshToken,user.Id });
        }
        
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshDto dto)
        {
            var token = await _context.RefreshTokens.FirstOrDefaultAsync(r => r.Token == dto.RefreshToken);
            if (token == null) return NotFound("Refresh token not found");

            token.IsRevoked = true;
            await _context.SaveChangesAsync();

            return Ok("Logged out successfully");
        }


        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshDto dto)
        {
            var oldToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == dto.RefreshToken);

            if (oldToken == null || oldToken.IsRevoked || oldToken.ExpiresAt < DateTime.UtcNow)
                return Unauthorized("Invalid refresh token");

            var user = await _context.Users.FindAsync(oldToken.UserId);
            if (user == null)
                return Unauthorized("User not found");

            // Отзываем старый токен
            oldToken.IsRevoked = true;

            // Генерируем новую пару токенов через AuthService
            var (accessToken, refreshToken) = await _authService.GenerateTokensAsync(user);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                accessToken,
                refreshToken,
                user.Id
            });
        }

        [HttpPost("validate")]
        public IActionResult ValidateToken([FromBody] TokenValidationDto dto)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]);

            try
            {
                tokenHandler.ValidateToken(dto.Token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _config["Jwt:Issuer"],
                    ValidAudience = _config["Jwt:Audience"],
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return Ok("Valid token");
            }
            catch
            {
                return Unauthorized("Invalid token");
            }
        }
    }
}
