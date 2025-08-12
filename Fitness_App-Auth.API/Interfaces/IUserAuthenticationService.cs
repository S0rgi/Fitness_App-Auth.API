using Fitness_App_Auth.API.Models;

namespace Fitness_App_Auth.API.Interfaces
{
    public interface IUserAuthenticationService
    {
        Task<TokenPair?> GenerateTokensAsync(User user);
    }

}
