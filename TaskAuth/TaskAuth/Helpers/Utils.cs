using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TaskAuth.Entities;
using TaskAuth.Models;

namespace TaskAuth.Helpers
{
    public class Utils
    {
        public enum RoleName
        {
            user,
            admin
        }

        private readonly IConfiguration _configuration;
        public Utils(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreateToken(UserModel user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:SymmetricSecurityKey").Value!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var token = new JwtSecurityToken(
                   claims: claims,
                   expires: DateTime.Now.AddMinutes(5),
                   signingCredentials: creds
               );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }

        public RefreshToken GetRefreshToken(UserModel user)
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
