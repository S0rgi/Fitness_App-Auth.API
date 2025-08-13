using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Gainly_Auth_API.Data;
using Gainly_Auth_API.Interfaces;
using Gainly_Auth_API.Models;

namespace Gainly_Auth_API.Service.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AuthDbContext _context;
        public UserRepository(AuthDbContext context)
        {
            _context = context;
        }

        public Task<bool> ExistsByEmailAsync(string email) => _context.Users.AnyAsync(u => u.Email == email);
        public Task<bool> ExistsByUsernameAsync(string username) => _context.Users.AnyAsync(u => u.Username == username);
        public Task<User?> FindByEmailAsync(string email) => _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        public Task<User?> FindByIdAsync(Guid id) => _context.Users.FindAsync(id).AsTask();
        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }
        public Task RemoveAsync(User user)
        {
            _context.Users.Remove(user);
            return Task.CompletedTask;
        }
        public Task SaveChangesAsync() => _context.SaveChangesAsync();
    }
}


