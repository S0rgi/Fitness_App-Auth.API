using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Gainly_Auth_API.Data;
using Gainly_Auth_API.Interfaces;
using Gainly_Auth_API.Models;

namespace Gainly_Auth_API.Service.Repositories
{
    public class FriendshipRepository : IFriendshipRepository
    {
        private readonly AuthDbContext _context;
        public FriendshipRepository(AuthDbContext context) { _context = context; }

        public Task<bool> FriendshipExistsAsync(Guid userId, Guid friendId, CancellationToken ct = default)
        {
            return _context.Friendships.AnyAsync(f =>
                (f.UserId == userId && f.FriendId == friendId) ||
                (f.UserId == friendId && f.FriendId == userId), ct);
        }

        public Task<Friendship?> FindByIdAsync(Guid friendshipId, CancellationToken ct = default)
        {
            return _context.Friendships
                .Include(f => f.Friend)
                .Include(f => f.User)
                .FirstOrDefaultAsync(f => f.Id == friendshipId, ct);
        }

        public async Task AddAsync(Friendship friendship, CancellationToken ct = default)
        {
            await _context.Friendships.AddAsync(friendship, ct);
        }

        public Task RemoveAsync(Friendship friendship, CancellationToken ct = default)
        {
            _context.Friendships.Remove(friendship);
            return Task.CompletedTask;
        }

        public async Task<IReadOnlyList<object>> GetPendingRequestsAsync(Guid userId, CancellationToken ct = default)
        {
            return await _context.Friendships
                .Include(f => f.User)
                .Where(f => f.FriendId == userId && f.Status == FriendshipStatus.Pending)
                .Select(f => new { f.Id, FromUserId = f.UserId, FromUsername = f.User.Username })
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<object>> GetFriendsAsync(Guid userId, CancellationToken ct = default)
        {
            return await _context.Friendships
                .Where(f => (f.UserId == userId || f.FriendId == userId) && f.Status == FriendshipStatus.Accepted)
                .Select(f => f.UserId == userId ? f.Friend : f.User)
                .Select(u => new { u.Id, u.Username, u.Email })
                .ToListAsync(ct);
        }

        public Task SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
    }
}


