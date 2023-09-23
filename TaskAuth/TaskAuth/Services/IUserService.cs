using TaskAuth.Entities;
using TaskAuth.Models;

namespace TaskAuth.Services
{
    public interface IUserService
    {
        Task<User> Register(SignupRequest request);
        Task<User?> GetUserByEmail(string Email);
        Task<User?> GetUserById(string id);
    }
}
