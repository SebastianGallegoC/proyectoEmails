using Domain.Interfaces;

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

        public async Task ExecuteAsync(string to, string subject, string body)
        {
            await _emailService.SendEmailAsync(to, subject, body);
            await _emailRepository.SaveEmailAsync(to, subject, body); 
        }
    }
}
