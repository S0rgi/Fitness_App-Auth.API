using Fitness_App_Auth.API.Grpc;
using Grpc.Core;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Fitness_App_Auth.API.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
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
