using Microsoft.EntityFrameworkCore;
using TaskAuth.Entities;
using TaskAuth.Helpers;
using TaskAuth.Models;

namespace TaskAuth.Services
{
    public class UserService : IUserService
    {

        private readonly TaskAuthContext _context;
        private readonly IRoleService _roleService;


        public UserService(TaskAuthContext context, IRoleService roleService)
        {
            _context = context;
            _roleService = roleService;
        }

        public async Task<User?> GetUserByEmail(string Email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.Equals(Email));
            return user;
        }

        public async Task<UserRegister> Register(UserDto request)
        {
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
            string roleName = Utils.RoleName.user.ToString();
            var role = await _roleService.GetRoleByName(roleName);
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                FullName = request.FullName,
                HashedPassword = hashedPassword,
                RoleId = role.Id
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return new UserRegister { Id = user.Id, Email = user.Email, FullName = user.FullName, Role = roleName };

        }

        public async Task AddRefreshToken(User user)
        {
            _context.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<User?> GetUserById(Guid id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id.Equals(id));
            return user;
        }

        public async Task<User?> GetUserByRefreshTokenId(int? id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshTokenId.Equals(id));
            return user;
        }
    }
}
