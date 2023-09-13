using TaskAuth.Entities;

namespace TaskAuth.Services
{
    public interface IRefreshTokenService
    {
        Task<RefreshToken> SaveRefreshToken(RefreshToken token);
        Task UpdateRefreshToken(RefreshToken token);
        Task<RefreshToken> GetRefreshTokenById(int? Id);
        Task DisuseChildrenRefreshTokenByParentId(int? Id);
        Task<RefreshToken> GetRefreshTokenByValue(string? Token);
        Task RevokeChildrenRefreshTokenByParentId(int? Id);
        Task DeleteChildrenRefreshTokenByParentId(int? Id);

    }
}
