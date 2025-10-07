using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services
{
    /// Caso de uso para envío de correos. Delegado a <see cref="IEmailService"/>.
    public sealed class EmailSenderUseCase
    {
        private readonly IEmailService _sender;

        public EmailSenderUseCase(IEmailService sender)
        {
            _sender = sender;
        }

 
        /// <param name="to">Destinatarios.</param>
        /// <param name="subject">Asunto.</param>
        /// <param name="body">Cuerpo (HTML permitido).</param>
        /// <param name="attachments">Adjuntos opcionales.</param>
        /// <param name="ct">Token de cancelación.</param>
        public Task ExecuteAsync(
            IEnumerable<string> to,
            string subject,
            string? body,
            IEnumerable<IFormFile>? attachments,
            CancellationToken ct = default)
        {
            return _sender.SendAsync(to, subject, body, attachments, ct);
        }
    }
}
