using TaskAuth.Entities;

namespace TaskAuth.Services
{
    public interface IRoleService
    {
        Task<Role> GetRoleByName(RoleName RoleName);
    }
}
