using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Gainly_Auth_API.Data;
using Gainly_Auth_API.Interfaces;
using Gainly_Auth_API.Models;

namespace Gainly_Auth_API.Service.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AuthDbContext _context;
        public RefreshTokenRepository(AuthDbContext context) { _context = context; }

        public Task<RefreshToken?> FindByTokenAsync(string token, CancellationToken ct = default) => _context.RefreshTokens.FirstOrDefaultAsync(r => r.Token == token, ct);
        public async Task AddAsync(RefreshToken token, CancellationToken ct = default)
        {
            await _context.RefreshTokens.AddAsync(token);
        }
        public Task SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
    }
}


