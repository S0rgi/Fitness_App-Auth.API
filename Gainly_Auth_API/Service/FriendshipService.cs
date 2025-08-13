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

    public async Task<(Friendship friendship, User sender, User friend)?> SendFriendRequestByUsernameAsync(Guid senderId, string friendUsername)
    {
        var sender = await _users.FindByIdAsync(senderId);
        var friend = await _context.Users.FirstOrDefaultAsync(u => u.Username == friendUsername);
        if (friend == null || sender == null || friend.Id == senderId)
            return null;

        var exists = await _friendships.FriendshipExistsAsync(senderId, friend.Id);
        if (exists)
            return null;

        var friendship = new Friendship
        {
            UserId = senderId,
            FriendId = friend.Id,
            Status = FriendshipStatus.Pending
        };
        await _friendships.AddAsync(friendship);
        await _friendships.SaveChangesAsync();

        var notification = new NotificationMessage
        {
            Type = "friend_invite",
            SenderName = sender?.Username ?? "Кто-то",
            RecipientEmail = friend.Email
        };
        await _publisher.PublishAsync(JsonSerializer.Serialize(notification));

        return (friendship, sender, friend);
    }

    public async Task<(Friendship friendship, User sender, User friend)?> RespondToFriendRequestAsync(Guid friendshipId, Guid userId, bool accept)
    {
        var friendship = await _friendships.FindByIdAsync(friendshipId);
        if (friendship == null || friendship.FriendId != userId)
            return null;

        friendship.Status = accept ? FriendshipStatus.Accepted : FriendshipStatus.Rejected;
        await _friendships.SaveChangesAsync();

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

    public async Task<IReadOnlyList<object>> GetPendingRequestsAsync(Guid userId)
    {
        return await _friendships.GetPendingRequestsAsync(userId);
    }

    public async Task<IReadOnlyList<object>> GetFriendsAsync(Guid userId)
    {
        return await _friendships.GetFriendsAsync(userId);
    }

    public async Task<bool> RemoveFriendAsync(Guid userId, string friendUsername)
    {
        var friend = await _context.Users.FirstOrDefaultAsync(u => u.Username == friendUsername);
        if (friend == null) return false;

        var friendship = await _context.Friendships.FirstOrDefaultAsync(f =>
            (f.UserId == userId && f.FriendId == friend.Id && f.Status == FriendshipStatus.Accepted) ||
            (f.UserId == friend.Id && f.FriendId == userId && f.Status == FriendshipStatus.Accepted));
        if (friendship == null) return false;

        await _friendships.RemoveAsync(friendship);
        await _friendships.SaveChangesAsync();
        return true;
    }
}




