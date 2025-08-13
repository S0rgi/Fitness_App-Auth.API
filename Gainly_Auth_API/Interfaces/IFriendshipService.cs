namespace Gainly_Auth_API.Interfaces;
using Gainly_Auth_API.Models;

public interface IFriendshipService
{
    Task<(Friendship friendship, User sender, User friend)?> SendFriendRequestByUsernameAsync(Guid senderId, string friendUsername);
    Task<(Friendship friendship, User sender, User friend)?> RespondToFriendRequestAsync(Guid friendshipId, Guid userId, bool accept);
    Task<IReadOnlyList<object>> GetPendingRequestsAsync(Guid userId);
    Task<IReadOnlyList<object>> GetFriendsAsync(Guid userId);
    Task<bool> RemoveFriendAsync(Guid userId, string friendUsername);
}




