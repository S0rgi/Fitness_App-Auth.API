namespace Fitness_App_Auth.API.Interfaces;
using Fitness_App_Auth.API.Models;

public interface IFriendshipService
{
    Task<(Friendship friendship, User sender, User friend)?> SendFriendRequestByUsernameAsync(Guid senderId, string friendUsername);
    Task<(Friendship friendship, User sender, User friend)?> RespondToFriendRequestAsync(Guid friendshipId, Guid userId, bool accept);
    Task<IReadOnlyList<object>> GetPendingRequestsAsync(Guid userId);
    Task<IReadOnlyList<object>> GetFriendsAsync(Guid userId);
    Task<bool> RemoveFriendAsync(Guid userId, string friendUsername);
}

