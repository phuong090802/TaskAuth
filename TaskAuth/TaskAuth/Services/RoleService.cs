using Microsoft.EntityFrameworkCore;
using TaskAuth.Entities;

namespace TaskAuth.Services
{
    public class RoleService : IRoleService
    {
        private readonly TaskAuthContext _context;

        public RoleService(TaskAuthContext context)
        {
            _context = context;
        }

        public async Task<Role> GetRoleByName(string RoleName)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName.Equals(RoleName));
            if (role is null)
            {
                return new Role { Id = 1, RoleName = RoleName };
            }
            return role;
        }
    }
}
