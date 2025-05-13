using Fitness_App_Auth.API.Grpc;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Fitness_App_Auth.API.Data;
public class UserGrpcService : UserService.UserServiceBase
{   
    private readonly AuthDbContext _context;

    public UserGrpcService(AuthDbContext context)
    {
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
}
