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
    public FriendshipService(AuthDbContext context, INotificationPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task<(Friendship friendship, User sender, User friend)?> SendFriendRequestByUsernameAsync(Guid senderId, string friendUsername)
    {
        var sender = await _context.Users.FindAsync(senderId);
        var friend = await _context.Users.FirstOrDefaultAsync(u => u.Username == friendUsername);
        if (friend == null || sender == null || friend.Id == senderId)
            return null;

        var exists = await _context.Friendships.AnyAsync(f =>
            (f.UserId == senderId && f.FriendId == friend.Id) ||
            (f.UserId == friend.Id && f.FriendId == senderId));
        if (exists)
            return null;

        var friendship = new Friendship
        {
            UserId = senderId,
            FriendId = friend.Id,
            Status = FriendshipStatus.Pending
        };
        _context.Friendships.Add(friendship);
        await _context.SaveChangesAsync();

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
        var friendship = await _context.Friendships
            .Include(f => f.Friend)
            .Include(f => f.User)
            .FirstOrDefaultAsync(f => f.Id == friendshipId);
        if (friendship == null || friendship.FriendId != userId)
            return null;

        friendship.Status = accept ? FriendshipStatus.Accepted : FriendshipStatus.Rejected;
        await _context.SaveChangesAsync();

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
        var pending = await _context.Friendships
            .Include(f => f.User)
            .Where(f => f.FriendId == userId && f.Status == FriendshipStatus.Pending)
            .Select(f => new { f.Id, FromUserId = f.UserId, FromUsername = f.User.Username })
            .ToListAsync();
        return pending;
    }

    public async Task<IReadOnlyList<object>> GetFriendsAsync(Guid userId)
    {
        var friends = await _context.Friendships
            .Where(f => (f.UserId == userId || f.FriendId == userId) && f.Status == FriendshipStatus.Accepted)
            .Select(f => f.UserId == userId ? f.Friend : f.User)
            .Select(u => new { u.Id, u.Username, u.Email })
            .ToListAsync();
        return friends;
    }

    public async Task<bool> RemoveFriendAsync(Guid userId, string friendUsername)
    {
        var friend = await _context.Users.FirstOrDefaultAsync(u => u.Username == friendUsername);
        if (friend == null) return false;

        var friendship = await _context.Friendships.FirstOrDefaultAsync(f =>
            (f.UserId == userId && f.FriendId == friend.Id && f.Status == FriendshipStatus.Accepted) ||
            (f.UserId == friend.Id && f.FriendId == userId && f.Status == FriendshipStatus.Accepted));
        if (friendship == null) return false;

        _context.Friendships.Remove(friendship);
        await _context.SaveChangesAsync();
        return true;
    }
}




