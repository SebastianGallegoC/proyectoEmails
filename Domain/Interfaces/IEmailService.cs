using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(
            IEnumerable<string> to,
            string subject,
            string? bodyHtml,
            IEnumerable<IFormFile>? attachments,
            CancellationToken ct = default);
    }
}
