using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using Models;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
//using proyectoEmails.Models;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    // Mala práctica: Variables "hardcodeadas" directamente en el controlador
    private const string _secret = "thisistheveryverylongandsupersecretkeythatisnotsecureinanyway";
    private const string _iss = "my-awesome-api";
    private const string _aud = "my-client";

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginData request)
    {
        // Mala práctica: Lógica de autenticación en el controlador
        if (request.Username != "admin" || request.Password != "1234")
        {
            return Unauthorized("Credenciales inválidas.");
        }

        // Mala práctica: Lógica de generación de token aquí mismo
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secret);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, request.Username),
                new Claim(ClaimTypes.Role, "Admin")
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = _iss,
            Audience = _aud,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return Ok(new { token = tokenHandler.WriteToken(token), expires = tokenDescriptor.Expires });
    }
}