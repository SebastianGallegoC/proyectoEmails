using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

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
        public async Task<IActionResult> SendEmail([FromForm] EmailRequest request)
        {
            if (request.To == null || !request.To.Any())
            {
                return BadRequest("Debe proporcionar al menos un destinatario.");
            }

            // Ejecutar el envío y esperar la finalización
            await _useCase.ExecuteAsync(request.To, request.Subject, request.Body, request.Attachments);

            return Ok("✅ Envío de correos completado.");
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