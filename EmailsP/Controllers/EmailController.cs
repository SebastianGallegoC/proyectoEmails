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

        public EmailController(
            EmailSenderUseCase useCase,
            IContactRepository contacts,
            ILogger<EmailController> logger)
        {
            _useCase = useCase;
            _contacts = contacts;
            _logger = logger;
        }

       
        [HttpPost("Send")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Consumes("multipart/form-data")] 
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Send([FromForm] EmailRequest req, CancellationToken ct)
        {
            if (req is null)
                return BadRequest(new ProblemDetails { Title = "Solicitud inválida", Detail = "El cuerpo de la solicitud es nulo." });

            
            var to = new List<string>();

            if (req.To is not null && req.To.Count > 0)
            {
                to.AddRange(req.To
                    .Where(e => !string.IsNullOrWhiteSpace(e))
                    .Select(e => e.Trim()));
            }

            if (req.ToContactIds is not null && req.ToContactIds.Count > 0)
            {
                var byIds = await _contacts.GetEmailsByIdsAsync(req.ToContactIds, ct);
                to.AddRange(byIds);
            }

            if (req.ToContactNames is not null && req.ToContactNames.Count > 0)
            {
                var byNames = await _contacts.GetEmailsByNamesAsync(req.ToContactNames, allowPartialMatch: true, ct);
                to.AddRange(byNames);
            }

            to = to
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Select(e => e.Trim())
                .Where(e => e.Length > 2)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (to.Count == 0)
            {
                return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
                {
                    ["recipients"] = new[] { "Debe incluir al menos un destinatario válido (To, ToContactNames o ToContactIds)." }
                }));
            }

            if (string.IsNullOrWhiteSpace(req.Subject) && string.IsNullOrWhiteSpace(req.Body))
            {
                return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
                {
                    ["content"] = new[] { "Debe especificar Subject o Body." }
                }));
            }

            try
            {
                await _useCase.ExecuteAsync(
                    to,
                    req.Subject!.Trim(),
                    string.IsNullOrWhiteSpace(req.Body) ? null : req.Body!.Trim(),
                    req.Attachments,
                    ct);

                return Ok(new
                {
                    status = "Queued",
                    recipients = to
                });
            }
            catch (OperationCanceledException)
            {
                return Problem(
                    title: "Cancelado",
                    detail: "La operación fue cancelada.",
                    statusCode: StatusCodes.Status499ClientClosedRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallo al enviar email");
                return Problem(
                    title: "Error al enviar email",
                    detail: Flatten(ex),
                    statusCode: StatusCodes.Status500InternalServerError);
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
