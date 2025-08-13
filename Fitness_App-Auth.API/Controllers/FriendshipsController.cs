using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Fitness_App_Auth.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
namespace Fitness_App_Auth.API.Controllers;
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
    public async Task<IActionResult> SendFriendRequestByUsername(string friendUsername)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        var result = await _friendshipService.SendFriendRequestByUsernameAsync(userId, friendUsername);
        if (result == null)
            return BadRequest("Невозможно отправить заявку.");
        var (friendship, user, friend) = result.Value;

        return Ok("Заявка отправлена.");
    }



    [Authorize]
    [HttpPost("respond/{friendshipId}")]
    public async Task<IActionResult> RespondToFriendRequest(Guid friendshipId, [FromQuery] bool accept)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var result = await _friendshipService.RespondToFriendRequestAsync(friendshipId, userId, accept);
        if (result == null)
            return NotFound("Заявка не найдена.");
        var (friendship, sender, friend) = result.Value;

        return Ok($"Заявка {(accept ? "принята" : "отклонена")}.");
    }


    [Authorize]
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingRequests()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var pending = await _friendshipService.GetPendingRequestsAsync(userId);
        return Ok(pending);
    }

    [Authorize]
    [HttpGet("list")]
    public async Task<IActionResult> GetFriends()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var friends = await _friendshipService.GetFriendsAsync(userId);
        return Ok(friends);
    }
    
    [Authorize]
    [HttpDelete("remove-friend/{friendUsername}")]
    public async Task<IActionResult> RemoveFriend(string friendUsername)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var ok = await _friendshipService.RemoveFriendAsync(userId, friendUsername);
        if (!ok) return BadRequest("Вы не являетесь друзьями.");
        return Ok("Пользователь удалён из друзей.");
    }

}
