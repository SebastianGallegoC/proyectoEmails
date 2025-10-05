using Application.DTOs;
using Application.Services;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EmailsP.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class EmailController : ControllerBase
    {
        private readonly EmailSenderUseCase _useCase;
        private readonly IContactRepository _contacts;
        private readonly ILogger<EmailController> _logger;
        private readonly IWebHostEnvironment _env;

        public EmailController(
            EmailSenderUseCase useCase,
            IContactRepository contacts,
            ILogger<EmailController> logger,
            IWebHostEnvironment env)
        {
            _useCase = useCase;
            _contacts = contacts;
            _logger = logger;
            _env = env;
        }

        [HttpPost("Send")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(100 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 100 * 1024 * 1024)]
        public async Task<IActionResult> SendEmail([FromForm] EmailRequest request)
        {
            // 1) Resolver destinatarios (To + nombres + ids)
            var recipients = new List<string>();

            if (request.To != null && request.To.Count > 0)
                recipients.AddRange(request.To.Where(x => !string.IsNullOrWhiteSpace(x)));

            if (request.ToContactNames != null && request.ToContactNames.Count > 0)
            {
                var emailsByNames = await _contacts.GetEmailsByNamesAsync(request.ToContactNames, allowPartialMatch: true);
                recipients.AddRange(emailsByNames);
                if (emailsByNames.Count == 0)
                    return BadRequest($"No se hallaron correos para los nombres: {string.Join(", ", request.ToContactNames)}");
            }

            if (request.ToContactIds != null && request.ToContactIds.Count > 0)
            {
                var emailsByIds = await _contacts.GetEmailsByIdsAsync(request.ToContactIds);
                recipients.AddRange(emailsByIds);
                if (emailsByIds.Count == 0)
                    return BadRequest("Ningún ID de contacto resolvió un correo.");
            }

            recipients = recipients
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (recipients.Count == 0)
                return BadRequest("Debe proporcionar al menos un destinatario (To, ToContactNames o ToContactIds).");

            // 2) Enviar
            try
            {
                await _useCase.ExecuteAsync(recipients, request.Subject, request.Body, request.Attachments);
                return Ok("✅ Envío de correos completado.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error controlado en envío.");
                var detail = _env.IsDevelopment() ? Flatten(ex) : ex.Message;
                // 502: Bad Gateway (fallo al hablar con servidor SMTP)
                return Problem(statusCode: StatusCodes.Status502BadGateway,
                               title: "Error al enviar",
                               detail: detail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al enviar.");
                var detail = _env.IsDevelopment() ? Flatten(ex) : "Ocurrió un error al enviar el correo.";
                return Problem(statusCode: StatusCodes.Status500InternalServerError,
                               title: "Error inesperado",
                               detail: detail);
            }
        }

        private static string Flatten(Exception ex)
        {
            var parts = new List<string>();
            for (var e = ex; e != null; e = e.InnerException)
            {
                if (!string.IsNullOrWhiteSpace(e.Message)) parts.Add(e.Message.Trim());
            }
            return string.Join(" | ", parts.Distinct());
        }
    }
}
