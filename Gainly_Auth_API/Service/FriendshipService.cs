using Gainly_Auth_API.Data;
using Gainly_Auth_API.Interfaces;
using Gainly_Auth_API.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Gainly_Auth_API.Service;

public class FriendshipService : IFriendshipService
{
    private readonly AuthDbContext _context;
    private readonly INotificationPublisher _publisher;
    private readonly IUserRepository _users;
    private readonly IFriendshipRepository _friendships;
    public FriendshipService(AuthDbContext context, INotificationPublisher publisher, IUserRepository users, IFriendshipRepository friendships)
    {
        _context = context;
        _publisher = publisher;
        _users = users;
        _friendships = friendships;
    }

    public async Task<(Friendship friendship, User sender, User friend)?> SendFriendRequestByUsernameAsync(Guid senderId, string friendUsername, CancellationToken cancellationToken = default)
    {
        var sender = await _users.FindByIdAsync(senderId, cancellationToken);
        var friend = await _users.FindByUsernameAsync(friendUsername, cancellationToken);
        if (friend == null || sender == null || friend.Id == senderId)
            return null;

        var exists = await _friendships.FriendshipExistsAsync(senderId, friend.Id, cancellationToken);
        if (exists)
            return null;

        var friendship = new Friendship
        {
            UserId = senderId,
            FriendId = friend.Id,
            Status = FriendshipStatus.Pending
        };
        await _friendships.AddAsync(friendship, cancellationToken);
        await _friendships.SaveChangesAsync(cancellationToken);

        var notification = new NotificationMessage
        {
            Type = "friend_invite",
            SenderName = sender?.Username ?? "Кто-то",
            RecipientEmail = friend.Email
        };
        await _publisher.PublishAsync(JsonSerializer.Serialize(notification));

        return (friendship, sender, friend);
    }

    public async Task<(Friendship friendship, User sender, User friend)?> RespondToFriendRequestAsync(Guid friendshipId, Guid userId, bool accept, CancellationToken cancellationToken = default)
    {
        var friendship = await _friendships.FindByIdAsync(friendshipId, cancellationToken);
        if (friendship == null || friendship.FriendId != userId)
            return null;

        friendship.Status = accept ? FriendshipStatus.Accepted : FriendshipStatus.Rejected;
        await _friendships.SaveChangesAsync(cancellationToken);

        var notification = new NotificationMessage
        {
            Type = "friend_response",
            SenderName = friendship.Friend.Username,
            RecipientEmail = friendship.User.Email,
            Action = friendship.Status.ToString()
        };
        await _publisher.PublishAsync(JsonSerializer.Serialize(notification));

        return (friendship, friendship.User, friendship.Friend);
    }

    public async Task<IReadOnlyList<object>> GetPendingRequestsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _friendships.GetPendingRequestsAsync(userId, cancellationToken);
    }

    public async Task<IReadOnlyList<object>> GetFriendsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _friendships.GetFriendsAsync(userId, cancellationToken);
    }

    public async Task<bool> RemoveFriendAsync(Guid userId, string friendUsername, CancellationToken cancellationToken = default)
    {
        var friend = await _users.FindByUsernameAsync(friendUsername, cancellationToken);
        if (friend == null) return false;

        var friendship = await _context.Friendships.FirstOrDefaultAsync(f =>
            (f.UserId == userId && f.FriendId == friend.Id && f.Status == FriendshipStatus.Accepted) ||
            (f.UserId == friend.Id && f.FriendId == userId && f.Status == FriendshipStatus.Accepted), cancellationToken);
        if (friendship == null) return false;

        await _friendships.RemoveAsync(friendship, cancellationToken);
        await _friendships.SaveChangesAsync(cancellationToken);
        return true;
    }
    public Task<List<User>> GetUsersAsync(string fuzzynick, CancellationToken cancellationToken = default)
    {
        return _users.GetUsersAsync(fuzzynick,cancellationToken );
    }

}




