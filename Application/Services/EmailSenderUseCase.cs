using Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Application.Services
{
    public class EmailSenderUseCase
    {
        private readonly IEmailService _emailService;
        private readonly IEmailRepository _emailRepository;

        public EmailSenderUseCase(IEmailService emailService, IEmailRepository emailRepository)
        {
            _emailService = emailService;
            _emailRepository = emailRepository;
        }

        public async Task ExecuteAsync(string to, string subject, string body, List<IFormFile> attachments = null)
        {
            var recipients = to.Split(',')
                              .Select(r => r.Trim())
                              .Where(r => !string.IsNullOrWhiteSpace(r))
                              .ToList();

            if (!recipients.Any())
            {
                throw new ArgumentException("Debe proporcionar al menos un destinatario válido.");
            }

            foreach (var recipient in recipients)
            {
                await _emailService.SendEmailAsync(recipient, subject, body, attachments);
                await _emailRepository.SaveEmailAsync(recipient, subject, body);
            }
        }
    }
}