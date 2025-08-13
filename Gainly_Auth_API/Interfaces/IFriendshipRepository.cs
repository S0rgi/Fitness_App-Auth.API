using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Gainly_Auth_API.Models;

namespace Gainly_Auth_API.Interfaces
{
    public interface IFriendshipRepository
    {
        Task<bool> FriendshipExistsAsync(Guid userId, Guid friendId);
        Task<Friendship?> FindByIdAsync(Guid friendshipId);
        Task AddAsync(Friendship friendship);
        Task RemoveAsync(Friendship friendship);
        Task<IReadOnlyList<object>> GetPendingRequestsAsync(Guid userId);
        Task<IReadOnlyList<object>> GetFriendsAsync(Guid userId);
        Task SaveChangesAsync();
    }
}


