using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace EmailsP.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly EmailSenderUseCase _useCase;
        private readonly AuthService _authService;

        public EmailController(EmailSenderUseCase useCase, AuthService authService)
        {
            _useCase = useCase;
            _authService = authService;
        }

        [HttpPost("Send")]
        [Authorize] // 🔐 Este endpoint ahora requiere un token JWT válido
        public async Task<IActionResult> SendEmail([FromBody] EmailRequest request)
        {
            await _useCase.ExecuteAsync(request.To, request.Subject, request.Body);
            return Ok("Correo enviado correctamente.");
        }

        [HttpPost("Login")]
        [AllowAnonymous] // Este sigue siendo público, para obtener el token
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var response = _authService.Authenticate(request);

            if (response == null)
                return Unauthorized("Credenciales inválidas");

            return Ok(response);
        }
    }
}
