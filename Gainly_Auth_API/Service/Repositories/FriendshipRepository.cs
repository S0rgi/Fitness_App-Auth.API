using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Gainly_Auth_API.Data;
using Gainly_Auth_API.Interfaces;
using Gainly_Auth_API.Models;
using Gainly_Auth_API.Dtos;

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

        public async Task<IReadOnlyList<FriendsRequestListDto>> GetPendingRequestsAsync(Guid userId, CancellationToken ct = default)
        {
            List<FriendsRequestListDto> res = await _context.Friendships
                .Include(f => f.User)
                .Where(f => f.FriendId == userId && f.Status == FriendshipStatus.Pending)
                .Select(f => new FriendsRequestListDto
                {
                    FriendshipId = f.Id,
                    FromUserId = f.UserId,
                    FromUsername = f.User.Username
                })
                .ToListAsync(ct);

            return res;
        }

        public async Task<IReadOnlyList<FuzzynickResponse>> GetFriendsAsync(Guid userId, CancellationToken ct = default)
        {
            var result = await _context.Friendships
                .Where(f => (f.UserId == userId || f.FriendId == userId) && f.Status == FriendshipStatus.Accepted)
                .Select(f => f.UserId == userId ? f.Friend : f.User)
                .Select(u => new FuzzynickResponse
                {
                    Id = u.Id,
                    Username = u.Username,
                    RegistrationDate = u.RegistrationDate
                })
                .ToListAsync(ct);

            return result;
        }

        public Task SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
    }
}


