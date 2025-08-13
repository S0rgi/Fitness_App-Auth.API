using Gainly_Auth_API.Grpc;
using Grpc.Core;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Gainly_Auth_API.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Gainly_Auth_API.Models;
using Gainly_Auth_API.Interfaces;
[AllowAnonymous]
public class UserGrpcService : UserService.UserServiceBase
{   
    private readonly IUserRepository _users;
    private readonly IFriendshipRepository _friendships;
    private readonly ITokenService _tokenService;
    public UserGrpcService(IUserRepository users, IFriendshipRepository friendships, ITokenService tokenService)
    {
        _users = users;
        _friendships = friendships;
        _tokenService = tokenService;
    }
public override async Task<FriendshipResponse> CheckFriendship(FriendshipRequest request, ServerCallContext context)
{
    if (!Guid.TryParse(request.UserId, out var userId))
        throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid UserId"));

    var friend = await _users.FindByUsernameAsync(request.FriendName);
    if (friend == null)
        throw new RpcException(new Status(StatusCode.NotFound, "Friend's name not found"));

    var exists = await _friendships.FriendshipExistsAsync(userId, friend.Id);
    if (!exists)
        throw new RpcException(new Status(StatusCode.NotFound, "User is not your friend"));

    return new FriendshipResponse
    {
        FriendId = friend.Id.ToString(),
        Email = friend.Email,
    };
}

    public override async Task<UserResponse> GetUserById(UserRequest request, ServerCallContext context)
    {
        var user = await _users.FindByIdAsync(Guid.Parse(request.Id));
        if (user == null)
            throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        return new UserResponse
        {
            Id = user.Id.ToString(),
            Email = user.Email,
            Username = user.Username
        };
    }
    [AllowAnonymous]
    public override async Task<UserResponse> ValidateToken(TokenRequest request, ServerCallContext context)
    {
        try
        {
            var claimsPrincipal = _tokenService.ValidateAccessToken(request.AccessToken);

            var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _users.FindByIdAsync(Guid.Parse(userId));

            if (user == null)
                throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

            return new UserResponse
            {
                Id = user.Id.ToString(),
                Email = user.Email,
                Username = user.Username
            };
        }
        catch
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid token"));
        }
    }

}



