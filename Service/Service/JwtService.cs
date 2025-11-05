using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Repo.IRepository;
// using BusinessObject.Models; // nhớ import model User/Role của bạn

namespace Service.Service
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public Task<string> GenerateJwtTokenAsync(User user, long roleId)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            // Đọc cấu hình
            var jwtSection = _configuration.GetSection("Jwt");
            var issuer = jwtSection["Issuer"];
            var audience = jwtSection["Audience"];
            var keyStr = jwtSection["Key"];
            var expireStr = jwtSection["ExpireMinutes"];

            if (string.IsNullOrWhiteSpace(issuer)) throw new InvalidOperationException("Jwt:Issuer is required.");
            if (string.IsNullOrWhiteSpace(audience)) throw new InvalidOperationException("Jwt:Audience is required.");
            if (string.IsNullOrWhiteSpace(keyStr) || keyStr.Length < 16)
                throw new InvalidOperationException("Jwt:Key is required (>=16 chars).");

            if (!int.TryParse(expireStr, out var expireMinutes) || expireMinutes <= 0)
                expireMinutes = 60;

            // Chốt role (ưu tiên tham số → user.RoleId → mặc định 2)
            var finalRoleId = roleId > 0 ? roleId : (user.RoleId > 0 ? user.RoleId : 2);

            // Claims chỉ dựa trên User
            var claims = new List<Claim>
        {
            new Claim("UserId", user.UserId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Username ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),

            // Bạn đang dùng số (1/2) cho vai trò → giữ nguyên
            new Claim(ClaimTypes.Role, finalRoleId.ToString()),
            new Claim("RoleId", finalRoleId.ToString()),

            // Các cờ trong model của bạn
            new Claim("IsPremium", user.IsPremium.ToString()),
            new Claim("IsEmailVerified", user.IsEmailVerified.ToString())
        };

            /*if (!string.IsNullOrWhiteSpace(user.GoogleID))
                claims.Add(new Claim("GoogleID", user.GoogleID));*/

            // Ký token
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyStr));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var nowUtc = DateTime.UtcNow;
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: nowUtc,
                expires: nowUtc.AddMinutes(expireMinutes),
                signingCredentials: creds
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return Task.FromResult(jwt);
        }
    }
}
