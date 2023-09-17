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

        public async Task DeleteChildrenRefreshTokenByParentId(int? Id)
        {
            var tokens = _context.RefreshTokens.Where(r => r.ParentId == Id);
            _context.RemoveRange(tokens);
            await _context.SaveChangesAsync();
        }

        public async Task DisuseChildrenRefreshTokenByParentId(int? Id)
        {
           _context.RefreshTokens.Where(r => r.ParentId == Id).ToList()
                .ForEach(rf =>
                {
                    if(rf.IsUsed)
                    {
                        rf.IsUsed = false;
                    }
                });
            await _context.SaveChangesAsync();
        }

        public async Task<RefreshToken> GetRefreshTokenById(int? Id)
        {
            var token = await _context.RefreshTokens.FirstOrDefaultAsync(r => r.Id == Id);
            return token!;
        }

        public async Task<RefreshToken?> GetRefreshTokenByValue(string? Token)
        {
            return await _context.RefreshTokens.FirstOrDefaultAsync(r => r.Token == Token);
        }

        public async Task RevokeChildrenRefreshTokenByParentId(int? Id)
        {
            _context.RefreshTokens.Where(rf => rf.ParentId == Id).ToList()
               .ForEach(rf =>
               {
                   if(!rf.IsRevoke)
                   {
                       rf.IsRevoke = true;
                   }
               });
            await _context.SaveChangesAsync();
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
