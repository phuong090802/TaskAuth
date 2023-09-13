using TaskAuth.Entities;
using TaskAuth.Models;

namespace TaskAuth.Services
{
    public interface IUserService
    {
        Task<UserRegister> Register(UserDto request);
        Task<User?> GetUserByEmail(string Email);
        Task AddRefreshToken(User user);
        Task<User?> GetUserById(Guid id);
        Task<User?> GetUserByRefreshTokenId(int? id);
    }
}
