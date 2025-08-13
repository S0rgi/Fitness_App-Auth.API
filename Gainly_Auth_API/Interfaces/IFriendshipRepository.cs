using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Gainly_Auth_API.Models;

namespace Gainly_Auth_API.Interfaces
{
    public interface IFriendshipRepository
    {
        Task<bool> FriendshipExistsAsync(Guid userId, Guid friendId, CancellationToken ct = default);
        Task<Friendship?> FindByIdAsync(Guid friendshipId, CancellationToken ct = default);
        Task AddAsync(Friendship friendship, CancellationToken ct = default);
        Task RemoveAsync(Friendship friendship, CancellationToken ct = default);
        Task<IReadOnlyList<object>> GetPendingRequestsAsync(Guid userId, CancellationToken ct = default);
        Task<IReadOnlyList<object>> GetFriendsAsync(Guid userId, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}


