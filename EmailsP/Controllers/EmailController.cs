using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmailsP.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly EmailSenderUseCase _useCase;

        public EmailController(EmailSenderUseCase useCase)
        {
            _useCase = useCase;
        }

        [HttpPost("Send")]
        public async Task<IActionResult> SendEmail([FromBody] EmailRequest request)
        {
            await _useCase.ExecuteAsync(request.To, request.Subject, request.Body);
            return Ok("Correo enviado correctamente.");
        }
    }
}
