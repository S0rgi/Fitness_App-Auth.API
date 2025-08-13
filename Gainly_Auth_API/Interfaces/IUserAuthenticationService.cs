using Gainly_Auth_API.Models;

namespace Gainly_Auth_API.Interfaces
{
    public interface IUserAuthenticationService
    {
        Task<TokenPair?> GenerateTokensAsync(User user);
    }

}



