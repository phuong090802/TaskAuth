using Microsoft.EntityFrameworkCore;
using NanoidDotNet;
using TaskAuth.Entities;
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
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == Email);
        }

        public async Task<User> Register(SignupRequest request)
        {
            var role = await _roleService.GetRoleByName(RoleName.User);
            var user = new User
            {
                Id = Nanoid.Generate(),
                Email = request.Email,
                FullName = request.FullName,
                HashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password),
                RoleId = role.Id
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;

        }

        public async Task<User?> GetUserById(string id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            return user;
        }

    }
}
