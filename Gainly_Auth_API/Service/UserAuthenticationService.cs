using Gainly_Auth_API.Data;
using Gainly_Auth_API.Interfaces;
using Gainly_Auth_API.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
namespace Gainly_Auth_API.Service
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

        public async Task<TokenPair?> GenerateTokensAsync(User user)
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

            return new TokenPair(accessToken, refreshToken);
        }
    }
}



