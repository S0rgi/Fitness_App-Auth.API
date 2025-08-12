using Fitness_App_Auth.API.Models;
using System.Security.Claims;

namespace Fitness_App_Auth.API.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(ClaimsIdentity identity);
        string GenerateRefreshToken();
        string GenerateJwtToken(User user);
        ClaimsPrincipal ValidateAccessToken(string token);
    }

}
