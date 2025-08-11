using Fitness_App_Auth.API.Models;

namespace Fitness_App_Auth.API.Interfaces
{
    public interface IUserAuthenticationService
    {
        Task<(string accessToken, string refreshToken)> GenerateTokensAsync(User user);
    }

}
