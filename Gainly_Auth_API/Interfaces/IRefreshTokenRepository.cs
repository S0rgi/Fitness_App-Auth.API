using System;
using System.Threading.Tasks;
using Gainly_Auth_API.Models;

namespace Gainly_Auth_API.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> FindByTokenAsync(string token, CancellationToken ct = default);
        Task AddAsync(RefreshToken token, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}


