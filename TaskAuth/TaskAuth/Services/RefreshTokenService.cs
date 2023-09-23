using Microsoft.EntityFrameworkCore;
using TaskAuth.Entities;

namespace TaskAuth.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly TaskAuthContext _context;

        public RefreshTokenService(TaskAuthContext context)
        {
            _context = context;
        }

        public async Task DeleteByToken(string token)
        {
            var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(r => r.Token == token);
            if (token is not null)
            {
                _context.RefreshTokens.Remove(refreshToken!);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteChildrenRefreshTokenByParentToken(string token)
        {
            var tokens = _context.RefreshTokens.Where(r => r.ParentToken == token);
            _context.RemoveRange(tokens);
            await _context.SaveChangesAsync();
        }

        public async Task<RefreshToken?> GetRefreshTokenByToken(string? Token)
        {
            return await _context.RefreshTokens.FirstOrDefaultAsync(r => r.Token == Token);
        }

        public async Task<RefreshToken> SaveRefreshToken(RefreshToken token)
        {
            _context.RefreshTokens.Add(token);
            await _context.SaveChangesAsync();
            var rereshToken = _context.RefreshTokens.Entry(token).Entity;
            return rereshToken;
        }

        public async Task UpdateRefreshToken(RefreshToken token)
        {
            _context.RefreshTokens.Update(token);
            await _context.SaveChangesAsync();
        }
    }
}
