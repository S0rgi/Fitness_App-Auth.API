using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Gainly_Auth_API.Models;

namespace Gainly_Auth_API.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
        Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default);
        Task<User?> FindByEmailAsync(string email, CancellationToken ct = default);
        Task<User?> FindByIdAsync(Guid id, CancellationToken ct = default);
        Task<User?> FindByUsernameAsync(string username, CancellationToken ct = default);
        Task AddAsync(User user, CancellationToken ct = default);
        Task RemoveAsync(User user, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}


