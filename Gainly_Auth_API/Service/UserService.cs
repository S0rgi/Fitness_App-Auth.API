using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Gainly_Auth_API.Data;
using Gainly_Auth_API.Interfaces;
using Gainly_Auth_API.Dtos;

namespace Gainly_Auth_API.Service
{
    public class UserService : IUserService
    {
        private readonly AuthDbContext _context;

        public UserService(AuthDbContext context)
        {
            _context = context;
        }

        public async Task<ChangeUsernameResult> ChangeUsernameAsync(Guid userId, string newUsername)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return ChangeUsernameResult.UserNotFound;
            }

            var exists = await _context.Users.AnyAsync(u => u.Username == newUsername);
            if (exists)
            {
                return ChangeUsernameResult.UsernameTaken;
            }

            user.Username = newUsername;
            await _context.SaveChangesAsync();
            return ChangeUsernameResult.Success;
        }

        public async Task<bool> DeleteUserByEmailAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return false;
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<GetMeResponse> GetMe(Guid userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            return new GetMeResponse()
            {
                nickname = user.Username,
                Email = user.Email,
                TGUsername = user.TGUsername,
                RegistrationDate = user.RegistrationDate
            };
        }


        public async Task<bool> UserExistsAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            return user != null;
        }
    }
}





