namespace Gainly_Auth_API.Interfaces;
using Gainly_Auth_API.Models;

public interface IFriendshipService
{
    Task<(Friendship friendship, User sender, User friend)?> SendFriendRequestByUsernameAsync(Guid senderId, string friendUsername, CancellationToken cancellationToken = default);
    Task<(Friendship friendship, User sender, User friend)?> RespondToFriendRequestAsync(Guid friendshipId, Guid userId, bool accept, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<object>> GetPendingRequestsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<object>> GetFriendsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> RemoveFriendAsync(Guid userId, string friendUsername, CancellationToken cancellationToken = default);
}




