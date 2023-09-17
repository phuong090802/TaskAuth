using TaskAuth.Entities;
using TaskAuth.Models;

namespace TaskAuth.Services
{
    public interface IUserService
    {
        Task<User> Register(SignupRequest request);
        Task<User?> GetUserByEmail(string Email);
        Task AddRefreshToken(User user);
        Task<User?> GetUserById(Guid id);
        Task<User?> GetUserByRefreshTokenId(int? id);
    }
}
