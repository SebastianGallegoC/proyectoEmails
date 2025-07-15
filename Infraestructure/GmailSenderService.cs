using Domain.Interfaces;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;

namespace Infrastructure.Services
{
    public class GmailSenderService : IEmailService
    {
        private readonly string _fromEmail = "waldovelcon@gmail.com";
        private readonly string _appPassword = "slkunloeyxtipiqw";
        private readonly IEmailRepository _emailRepository;

        public GmailSenderService(IEmailRepository emailRepository)
        {
            _emailRepository = emailRepository;
        }

        public async Task SendEmailAsync(string toList, string subject, string body)
        {
            var recipients = toList.Split(',')
                                   .Select(r => r.Trim())
                                   .Where(r => !string.IsNullOrWhiteSpace(r))
                                   .ToList();

            string htmlTemplate = @"
<!DOCTYPE html>
<html lang=""es"">
<head>
    <meta charset=""UTF-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{{subject}}</title>
    <style type=""text/css"">
        body { margin: 0; padding: 0; background-color: #f5f7fa; font-family: 'Google Sans', 'Roboto', Arial, sans-serif; color: #333; -webkit-font-smoothing: antialiased; width: 100% !important; }
        p { margin: 0; padding: 0; }
        a { text-decoration: none; }
        img { display: block; border: 0; height: auto; line-height: 100%; outline: none; text-decoration: none; }
        table { border-collapse: collapse !important; }
        @media only screen and (max-width: 600px) {
            .container { width: 100% !important; max-width: 100% !important; }
            .content-padding { padding: 20px !important; }
        }
    </style>
</head>
<body>
    <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
        <tr>
            <td align=""center"">
                <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""600"" class=""container"" style=""background-color: #ffffff; border-radius: 8px;"">
                    <tr>
                        <td align=""center"" style=""background-color: #e80414;"">
                            <img src=""https://i.ytimg.com/vi/Rmmf0QP0X8g/maxresdefault.jpg"" alt=""Encabezado del Correo"" width=""600"" style=""max-width: 100%; border-radius: 8px 8px 0 0;"" />
                        </td>
                    </tr>
                    <tr>
                        <td class=""content-padding"" style=""padding: 30px;"">
                            <p style=""font-size: 16px;"">Hola:</p>
                            <p style=""font-size: 15px;"">{{body}}</p>
                            <p style=""font-size: 15px;"">Un afectuoso saludo,</p>
                            <p style=""font-size: 15px;"">[Tu Nombre]</p>
                        </td>
                    </tr>
                    <tr>
                        <td align=""center"" style=""background-color: #212121; padding: 30px; color: #bdbdbd; font-size: 12px;"">
                            <p>Este mensaje fue generado automáticamente. Por favor, no responder.</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_fromEmail, _appPassword);

            foreach (var to in recipients)
            {
                for (int i = 0; i < 2; i++)
                {
                    try
                    {
                        string htmlBody = htmlTemplate
                            .Replace("{{to}}", to)
                            .Replace("{{subject}}", subject)
                            .Replace("{{body}}", body);

                        var message = new MimeMessage();
                        message.From.Add(MailboxAddress.Parse(_fromEmail));
                        message.To.Add(MailboxAddress.Parse(to));
                        message.Subject = subject;
                        message.Body = new TextPart(TextFormat.Html) { Text = htmlBody };

                        await smtp.SendAsync(message);
                        await _emailRepository.SaveEmailAsync(to, subject, body);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error al enviar a {to} (intento {i + 1}): {ex.Message}");
                        continue;
                    }
                }
            }

            await smtp.DisconnectAsync(true);
        }
    }
}