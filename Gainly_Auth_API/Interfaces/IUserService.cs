using System;
using System.Threading.Tasks;
using Gainly_Auth_API.Dtos;

namespace Gainly_Auth_API.Interfaces
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
        Task<GetMeResponse> GetMe(Guid userId);

        Task<bool> UserExistsAsync(string email);
    }
}





