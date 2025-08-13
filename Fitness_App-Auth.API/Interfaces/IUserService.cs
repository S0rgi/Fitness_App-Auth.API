using System;
using System.Threading.Tasks;

namespace Fitness_App_Auth.API.Interfaces
{
    public enum ChangeUsernameResult
    {
        Success,
        UserNotFound,
        UsernameTaken
    }

    public interface IUserService
    {
        Task<ChangeUsernameResult> ChangeUsernameAsync(Guid userId, string newUsername);
        Task<bool> DeleteUserByEmailAsync(string email);
        Task<bool> UserExistsAsync(string email);
    }
}


