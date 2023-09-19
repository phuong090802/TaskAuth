using Microsoft.EntityFrameworkCore;
using NanoidDotNet;
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

        public async Task DeleteById(string Id)
        {
            var token = await _context.RefreshTokens.FirstOrDefaultAsync(r => r.Id == Id);
            if (token is not null)
            {
                _context.RefreshTokens.Remove(token);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteChildrenRefreshTokenByParentId(string Id)
        {
            var tokens = _context.RefreshTokens.Where(r => r.ParentId == Id);
            _context.RemoveRange(tokens);
            await _context.SaveChangesAsync();
        }

        public async Task DisuseChildrenRefreshTokenByParentId(string? Id)
        {
            _context.RefreshTokens.Where(r => r.ParentId == Id).ToList()
                 .ForEach(rf =>
                 {
                     if (rf.IsUsed)
                     {
                         rf.IsUsed = false;
                     }
                 });
            await _context.SaveChangesAsync();
        }

        public async Task<RefreshToken> GetRefreshTokenById(string? Id)
        {
            var token = await _context.RefreshTokens.FirstOrDefaultAsync(r => r.Id == Id);
            return token!;
        }

        public async Task<RefreshToken?> GetRefreshTokenByValue(string? Token)
        {
            return await _context.RefreshTokens.FirstOrDefaultAsync(r => r.Token == Token);
        }

        public async Task<RefreshToken?> GetRefreshTokenInBrachIsRevoke(string Id)
        {
            return await _context.RefreshTokens.FirstOrDefaultAsync(r => r.ParentId == Id && r.IsRevoke);
        }

        public async Task RevokeChildrenRefreshTokenByParentId(string? Id)
        {
            _context.RefreshTokens.Where(rf => rf.ParentId == Id).ToList()
               .ForEach(rf =>
               {
                   if (!rf.IsRevoke)
                   {
                       rf.IsRevoke = true;
                   }
               });
            await _context.SaveChangesAsync();
        }

        public async Task<RefreshToken> SaveRefreshToken(RefreshToken token)
        {
            token.Id = Nanoid.Generate();
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
