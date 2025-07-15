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
        [Authorize] // 🔐 Este endpoint requiere token JWT válido
        public IActionResult SendEmail([FromBody] EmailRequest request)
        {
            // Ejecutar el envío en segundo plano para no bloquear al cliente
            _ = Task.Run(() => _useCase.ExecuteAsync(request.To, request.Subject, request.Body));

            // Respuesta inmediata
            return Ok("✅ Envío de correo iniciado. Puedes continuar.");
        }

        [HttpPost("Login")]
        [AllowAnonymous] // Público para obtener el token
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var response = _authService.Authenticate(request);

            if (response == null)
                return Unauthorized("Credenciales inválidas");

            return Ok(response);
        }
    }
}