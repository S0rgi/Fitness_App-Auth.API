using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Gainly_Auth_API.Interfaces;
using Microsoft.AspNetCore.Authorization;
namespace Gainly_Auth_API.Controllers;
[ApiController]
[Route("api/friends")]
public class FriendController : ControllerBase
{
    private readonly IFriendshipService _friendshipService;

    public FriendController(IFriendshipService friendshipService)
    {
        _friendshipService = friendshipService;
    }

    [Authorize]
    [HttpPost("send-request-by-username/{friendUsername}")]
    public async Task<IActionResult> SendFriendRequestByUsername(string friendUsername, CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(sub, out var userId))
            return Problem(title: "Unauthorized", statusCode: StatusCodes.Status401Unauthorized);

        var result = await _friendshipService.SendFriendRequestByUsernameAsync(userId, friendUsername, ct);
        if (result == null)
            return Problem(title: "Bad Request", detail: "Невозможно отправить заявку.", statusCode: StatusCodes.Status400BadRequest);
        var (friendship, user, friend) = result.Value;

        return Ok("Заявка отправлена.");
    }



    [Authorize]
    [HttpPost("respond/{friendshipId}")]
    public async Task<IActionResult> RespondToFriendRequest(Guid friendshipId, [FromQuery] bool accept, CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(sub, out var userId))
            return Problem(title: "Unauthorized", statusCode: StatusCodes.Status401Unauthorized);
        var result = await _friendshipService.RespondToFriendRequestAsync(friendshipId, userId, accept, ct);
        if (result == null)
            return Problem(title: "Not Found", detail: "Заявка не найдена.", statusCode: StatusCodes.Status404NotFound);
        var (friendship, sender, friend) = result.Value;

        return Ok($"Заявка {(accept ? "принята" : "отклонена")}.");
    }


    [Authorize]
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingRequests(CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(sub, out var userId))
            return Problem(title: "Unauthorized", statusCode: StatusCodes.Status401Unauthorized);
        var pending = await _friendshipService.GetPendingRequestsAsync(userId, ct);
        return Ok(pending);
    }

    [Authorize]
    [HttpGet("list")]
    public async Task<IActionResult> GetFriends(CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(sub, out var userId))
            return Problem(title: "Unauthorized", statusCode: StatusCodes.Status401Unauthorized);
        var friends = await _friendshipService.GetFriendsAsync(userId, ct);
        return Ok(friends);
    }
    
    [Authorize]
    [HttpDelete("remove-friend/{friendUsername}")]
    public async Task<IActionResult> RemoveFriend(string friendUsername, CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(sub, out var userId))
            return Problem(title: "Unauthorized", statusCode: StatusCodes.Status401Unauthorized);
        var ok = await _friendshipService.RemoveFriendAsync(userId, friendUsername, ct);
        if (!ok) return Problem(title: "Bad Request", detail: "Вы не являетесь друзьями.", statusCode: StatusCodes.Status400BadRequest);
        return Ok("Пользователь удалён из друзей.");
    }

}



