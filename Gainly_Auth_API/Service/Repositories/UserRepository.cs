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

        public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default) => _context.Users.AnyAsync(u => u.Email == email, ct);
        public Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default) => _context.Users.AnyAsync(u => u.Username == username, ct);
        public Task<User?> FindByEmailAsync(string email, CancellationToken ct = default) => _context.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        public Task<User?> FindByIdAsync(Guid id, CancellationToken ct = default) => _context.Users.FindAsync([id], ct).AsTask();
        public Task<User?> FindByUsernameAsync(string username, CancellationToken ct = default) => _context.Users.FirstOrDefaultAsync(u => u.Username == username, ct);
        public Task<User?> FindByTgLoginAsync(string TGUsername, CancellationToken ct = default)=>_context.Users.FirstOrDefaultAsync(u => u.TGUsername == TGUsername, ct);
        public async Task AddEmailConfirmationAsync(EmailConfirmation confirmation, CancellationToken ct = default)
        {
            await _context.EmailConfirmations.AddAsync(confirmation, ct);
        }

        public Task<EmailConfirmation?> GetEmailConfirmationAsync(string email, string code, CancellationToken ct = default)
        {
            return _context.EmailConfirmations
                .FirstOrDefaultAsync(e => e.Email == email && e.Code == code, ct);
        }

        public Task<EmailConfirmation?> GetLastEmailConfirmationAsync(string email, CancellationToken ct = default)
        {
        return _context.EmailConfirmations
            .OrderByDescending(e => e.CreatedAt)
            .FirstOrDefaultAsync(e => e.Email == email, ct);
        }
        public async Task AddAsync(User user, CancellationToken ct = default)
        {
            await _context.Users.AddAsync(user);
        }
        public Task RemoveAsync(User user, CancellationToken ct = default)
        {
            _context.Users.Remove(user);
            return Task.CompletedTask;
        }
        public Task SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);

        public async Task<List<User>> GetUsersAsync(string fuzzynick, CancellationToken ct = default)
        {
            var limit = 5;
            var users = await _context.Users
                .Where(u => EF.Functions.TrigramsSimilarity(u.Username, fuzzynick) > 0.3)
                .OrderByDescending(u => EF.Functions.TrigramsSimilarity(u.Username, fuzzynick))
                .Take(limit)
                .ToListAsync();
            return users;
        }

    }
}


