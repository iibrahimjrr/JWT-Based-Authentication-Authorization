using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JwtAuthDemo.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace JwtAuthDemo.Services
{
    public class JwtService
    {
        private readonly IConfiguration _config;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly string _secret;
        private readonly int _expiryMinutes;

        public JwtService(IConfiguration config)
        {
            _config = config;
            _issuer = _config["JwtSettings:Issuer"]!;
            _audience = _config["JwtSettings:Audience"]!;
            _secret = _config["JwtSettings:Secret"]!;
            _expiryMinutes = int.Parse(_config["JwtSettings:ExpiryMinutes"] ?? "60");
        }

        public (string token, DateTime expires) GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var expires = DateTime.UtcNow.AddMinutes(_expiryMinutes);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);
            return (tokenStr, expires);
        }
    }
}
