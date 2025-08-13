using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Gainly_Auth_API.Models;

namespace Gainly_Auth_API.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> ExistsByEmailAsync(string email);
        Task<bool> ExistsByUsernameAsync(string username);
        Task<User?> FindByEmailAsync(string email);
        Task<User?> FindByIdAsync(Guid id);
        Task AddAsync(User user);
        Task RemoveAsync(User user);
        Task SaveChangesAsync();
    }
}


