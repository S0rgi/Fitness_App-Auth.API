using Gainly_Auth_API.Models;
using System.Security.Claims;

namespace Gainly_Auth_API.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(ClaimsIdentity identity);
        string GenerateRefreshToken();
        string GenerateJwtToken(User user);
        ClaimsPrincipal ValidateAccessToken(string token);
    }

}



