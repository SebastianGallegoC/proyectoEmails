using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Domain.Interfaces;

namespace Infrastructure.Services
{
    /// <summary>
    /// Servicio “nulo” para pruebas: NO envía correos, solo simula.
    /// Útil para comprobar que el upload de adjuntos no tumba el host.
    /// </summary>
    public class NullEmailService : IEmailService
    {
        private readonly ILogger<NullEmailService> _logger;

        public NullEmailService(ILogger<NullEmailService> logger)
        {
            _logger = logger;
        }

        public async Task SendAsync(
            IEnumerable<string> to,
            string subject,
            string? bodyHtml,
            IEnumerable<IFormFile>? attachments,
            CancellationToken ct = default)
        {
            _logger.LogInformation(
                "Simulación envío. To={To} Subject={Subject} Adjuntos={Count}",
                string.Join(",", to ?? Enumerable.Empty<string>()),
                subject,
                attachments?.Count() ?? 0
            );

            // Leer superficialmente los streams para validar uploads (no hace nada con ellos)
            if (attachments != null)
            {
                foreach (var f in attachments)
                {
                    using var _ = f.OpenReadStream();
                }
            }

            await Task.CompletedTask;
        }
    }
}
