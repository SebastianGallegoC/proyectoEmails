using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services
{
    public class EmailSenderUseCase
    {
        private readonly IEmailService _sender;

        public EmailSenderUseCase(IEmailService sender)
        {
            _sender = sender;
        }

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
