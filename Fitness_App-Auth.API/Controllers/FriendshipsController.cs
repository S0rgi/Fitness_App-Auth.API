using Fitness_App_Auth.API.Data;
using Fitness_App_Auth.API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Fitness_App_Auth.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
namespace Fitness_App_Auth.API.Controllers;
[ApiController]
[Route("api/friends")]
public class FriendController : ControllerBase
{
    private readonly AuthDbContext _context;

    private readonly INotificationPublisher _publisher;

    public FriendController( AuthDbContext context, INotificationPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    [Authorize]
    [HttpPost("send-request-by-username/{friendUsername}")]
    public async Task<IActionResult> SendFriendRequestByUsername(string friendUsername)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        var user = await _context.Users.FindAsync(userId);
        var friend = await _context.Users.FirstOrDefaultAsync(u => u.Username == friendUsername);

        if (friend == null)
            return NotFound("Пользователь с таким именем не найден.");

        if (friend.Id == userId)
            return BadRequest("Нельзя отправить заявку самому себе.");

        var exists = await _context.Friendships.AnyAsync(f =>
            (f.UserId == userId && f.FriendId == friend.Id) ||
            (f.UserId == friend.Id && f.FriendId == userId));

        if (exists)
            return BadRequest("Заявка уже существует или вы уже друзья.");

        var friendship = new Friendship
        {
            UserId = userId,
            FriendId = friend.Id,
            Status = FriendshipStatus.Pending
        };

        _context.Friendships.Add(friendship);
        await _context.SaveChangesAsync();

        // Отправка уведомления
        var notification = new NotificationMessage
        {
            Type = "friend_invite",
            SenderName = user?.Username ?? "Кто-то",
            RecipientEmail = friend.Email
        };
        await _publisher.PublishAsync(JsonSerializer.Serialize(notification));

        return Ok("Заявка отправлена.");
    }



    [Authorize]
    [HttpPost("respond/{friendshipId}")]
    public async Task<IActionResult> RespondToFriendRequest(Guid friendshipId, [FromQuery] bool accept)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        var friendship = await _context.Friendships
            .Include(f => f.Friend)
            .Include(f => f.User)
            .FirstOrDefaultAsync(f => f.Id == friendshipId);

        if (friendship == null || friendship.FriendId != userId)
            return NotFound("Заявка не найдена.");

        friendship.Status = accept ? FriendshipStatus.Accepted : FriendshipStatus.Rejected;
        await _context.SaveChangesAsync();

        // Уведомление отправителю
        var notification = new NotificationMessage
        {
            Type = "friend_response",
            SenderName = friendship.Friend.Username,
            RecipientEmail = friendship.User.Email,
            Action = friendship.Status.ToString()
        };
        await _publisher.PublishAsync(JsonSerializer.Serialize(notification));

        return Ok($"Заявка {(accept ? "принята" : "отклонена")}.");
    }


    [Authorize]
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingRequests()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var pending = await _context.Friendships
            .Include(f => f.User)
            .Where(f => f.FriendId == userId && f.Status == FriendshipStatus.Pending)
            .Select(f => new {
                f.Id,
                FromUserId = f.UserId,
                FromUsername = f.User.Username
            })
            .ToListAsync();

        return Ok(pending);
    }

    [Authorize]
    [HttpGet("list")]
    public async Task<IActionResult> GetFriends()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        var friends = await _context.Friendships
            .Where(f =>
                (f.UserId == userId || f.FriendId == userId) &&
                f.Status == FriendshipStatus.Accepted)
            .Select(f => f.UserId == userId ? f.Friend : f.User)
            .Select(u => new {
                u.Id,
                u.Username,
                u.Email
            })
            .ToListAsync();

        return Ok(friends);
    }
    
    [Authorize]
    [HttpDelete("remove-friend/{friendUsername}")]
    public async Task<IActionResult> RemoveFriend(string friendUsername)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        var friend = await _context.Users.FirstOrDefaultAsync(u => u.Username == friendUsername);
        if (friend == null)
            return NotFound("Пользователь не найден.");

        var friendship = await _context.Friendships.FirstOrDefaultAsync(f =>
            (f.UserId == userId && f.FriendId == friend.Id && f.Status == FriendshipStatus.Accepted) ||
            (f.UserId == friend.Id && f.FriendId == userId && f.Status == FriendshipStatus.Accepted));

        if (friendship == null)
            return BadRequest("Вы не являетесь друзьями.");

        _context.Friendships.Remove(friendship);
        await _context.SaveChangesAsync();

        return Ok("Пользователь удалён из друзей.");
    }

}
