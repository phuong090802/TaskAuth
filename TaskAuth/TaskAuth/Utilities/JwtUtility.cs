using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TaskAuth.Entities;

namespace TaskAuth.Helpers
{
    public class JwtUtility
    {

        private readonly IConfiguration _configuration;
        public JwtUtility(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(User user)
        {
            string roleName = user.RoleId == 1 ? RoleName.user.ToString() : RoleName.admin.ToString();
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, roleName),
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _configuration.GetSection("AppSettings:SymmetricSecurityKey")
                    .Value!));

            var creds = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256Signature);

            var token = new JwtSecurityToken(
                   claims: claims,
                   expires: DateTime.Now.AddMinutes(5),
                   signingCredentials: creds
               );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }

        public RefreshToken GenerateRefreshToken()
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                IsUsedAt = DateTime.Now,
                IsExpiredAt = DateTime.Now.AddDays(7),
                IsRevoke = false,
                IsUsed = true
            };
            return refreshToken;
        }
    }
}
