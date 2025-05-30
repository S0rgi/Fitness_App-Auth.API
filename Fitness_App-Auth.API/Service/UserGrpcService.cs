using Fitness_App_Auth.API.Grpc;
using Grpc.Core;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Fitness_App_Auth.API.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Fitness_App_Auth.API.Models;
[AllowAnonymous]
public class UserGrpcService : UserService.UserServiceBase
{   
    private readonly AuthDbContext _context;
    private readonly IConfiguration _config;
    public UserGrpcService(AuthDbContext context,IConfiguration config)
    {
        _config = config;
        _context = context;
    }
public override async Task<FriendshipResponse> CheckFriendship(FriendshipRequest request, ServerCallContext context)
{
    if (!Guid.TryParse(request.UserId, out var userId))
        throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid UserId"));

    var friend = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.FriendName);
    if (friend == null)
        throw new RpcException(new Status(StatusCode.NotFound, "Friend's name not found"));

    var exists = await _context.Friendships.AnyAsync(f =>
        (f.UserId == userId && f.FriendId == friend.Id ||
        f.UserId == friend.Id && f.FriendId == userId) &&
        f.Status == FriendshipStatus.Accepted);
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
        var user = await _context.Users.FindAsync(Guid.Parse(request.Id));
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
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]);

    try
    {
        var claims = tokenHandler.ValidateToken(request.AccessToken, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = _config["Jwt:Issuer"],
            ValidAudience = _config["Jwt:Audience"],
            ClockSkew = TimeSpan.Zero
        }, out _);

        var userId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _context.Users.FindAsync(Guid.Parse(userId));

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
