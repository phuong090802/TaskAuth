using TaskAuth.Entities;

namespace TaskAuth.Services
{
    public interface IRefreshTokenService
    {
        Task<RefreshToken> SaveRefreshToken(RefreshToken token);
        Task UpdateRefreshToken(RefreshToken token);
        Task<RefreshToken> GetRefreshTokenById(string? Id);
        Task DisuseChildrenRefreshTokenByParentId(string? Id);
        Task<RefreshToken?> GetRefreshTokenByValue(string? Token);
        Task RevokeChildrenRefreshTokenByParentId(string? Id);
        Task DeleteChildrenRefreshTokenByParentId(string Id);
        Task<RefreshToken?> GetRefreshTokenInBrachIsRevoke(string Id);
        Task DeleteById(string Id);

    }
}
