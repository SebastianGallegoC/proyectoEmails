using Application.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Application.Services
{
    public class AuthService
    {
        private readonly IConfiguration _configuration;

        public AuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public LoginResponse Authenticate(LoginRequest request)
        {
            // 🔐 Validación de usuario simulada (puedes conectar a BD luego)
            if (request.Username != "admin" || request.Password != "1234")
                return null;

            // 🪪 Crear claims (datos que irán dentro del token)
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, request.Username),
                new Claim(ClaimTypes.Role, "Admin")
            };

            // 🔑 Clave secreta desde appsettings.json
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 📅 Crear el token
            var expiration = DateTime.UtcNow.AddHours(1);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiration,
                signingCredentials: credentials
            );

            // 📦 Devolver el token como respuesta
            return new LoginResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expiration
            };
        }
    }
}
