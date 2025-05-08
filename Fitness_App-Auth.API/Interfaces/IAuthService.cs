using Fitness_App_Auth.API.Models;

namespace Fitness_App_Auth.API.Interfaces
{
    public interface IAuthService
    {
        Task<(string accessToken, string refreshToken)> GenerateTokensAsync(User user);
    }

}
