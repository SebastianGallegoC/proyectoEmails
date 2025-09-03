using Microsoft.AspNetCore.Mvc;
using Application.DTOs;
using Application.Services;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(IConfiguration configuration)
    {
        _authService = new AuthService(configuration);
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var result = _authService.Authenticate(request);
        if (result == null)
            return Unauthorized("Credenciales inválidas");

        return Ok(result);
    }
}
