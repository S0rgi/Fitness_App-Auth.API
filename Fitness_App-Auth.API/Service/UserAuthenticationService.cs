using Fitness_App_Auth.API.Data;
using Fitness_App_Auth.API.Interfaces;
using Fitness_App_Auth.API.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
namespace Fitness_App_Auth.API.Service
{
    public class UserAuthenticationService : IUserAuthenticationService
    {
        private readonly ITokenService _tokenService;
        private readonly AuthDbContext _context;
        private readonly JwtOptions _jwtOptions;
        public UserAuthenticationService(ITokenService tokenService, AuthDbContext context,IOptions<JwtOptions> jwtOptions)
        {
            _tokenService = tokenService;
            _context = context;
                        _jwtOptions = jwtOptions.Value;
        }

        public async Task<(string accessToken, string refreshToken)> GenerateTokensAsync(User user)
        {
            var identity = new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        });

            var accessToken = _tokenService.GenerateAccessToken(identity);
            var refreshToken = _tokenService.GenerateRefreshToken();

            await _context.RefreshTokens.AddAsync(new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenLifetimeDays)
            });

            await _context.SaveChangesAsync();

            return (accessToken, refreshToken);
        }
    }
}
