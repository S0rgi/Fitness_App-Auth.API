using Gainly_Auth_API.Data;
using Gainly_Auth_API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Gainly_Auth_API.Service{
    public class UsernameGenerator : IUsernameGenerator
{
    private readonly AuthDbContext _context;

    public UsernameGenerator(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateAsync(string email)
    {
        var baseName = email.Split('@')[0]
                            .Replace(".", "_")
                            .Replace("-", "_")
                            .ToLower();

        string username;
        var rng = new Random();

        do
        {
            var suffix = rng.Next(1000, 9999);
            username = $"{baseName}_{suffix}";
        }
        while (await _context.Users.AnyAsync(u => u.Username == username));

        return username;
    }
}

}


