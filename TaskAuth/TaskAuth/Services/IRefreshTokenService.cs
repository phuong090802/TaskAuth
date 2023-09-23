using TaskAuth.Entities;

namespace TaskAuth.Services
{
    public interface IRefreshTokenService
    {
        Task<RefreshToken> SaveRefreshToken(RefreshToken token);
        Task UpdateRefreshToken(RefreshToken token);
        Task<RefreshToken?> GetRefreshTokenByToken(string? Token);
        Task DeleteChildrenRefreshTokenByParentToken(string token);
        Task DeleteByToken(string token);

    }
}
