using Domain.Interfaces;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;

namespace Infrastructure.Services
{
    public class GmailSenderService : IEmailService
    {
        private readonly string _fromEmail = "waldovelcon@gmail.com"; // Reemplaza
        private readonly string _appPassword = "slkunloeyxtipiqw"; // Reemplaza

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            // Leer plantilla HTML
            string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "EmailTemplate.html");
            string htmlTemplate = await File.ReadAllTextAsync(templatePath);

            // Reemplazar los placeholders
            string htmlBody = htmlTemplate
                .Replace("{{to}}", to)
                .Replace("{{subject}}", subject)
                .Replace("{{body}}", body);

            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(_fromEmail));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new TextPart(TextFormat.Html) { Text = htmlBody };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_fromEmail, _appPassword);
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);
        }
    }
}
