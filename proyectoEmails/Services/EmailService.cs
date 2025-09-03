using Npgsql;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

public class EmailService
{
    private readonly string _fromEmail = "waldovelcon@gmail.com";
    private readonly string _appPassword = "slkunloeyxtipiqw";
    private readonly string _connectionString = "Host=localhost;Database=emails;Username=postgres;Password=123";

    private readonly string _htmlTemplate = @"
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

    public async Task SendAndSaveEmailAsync(string to, string subject, string body, List<IFormFile> attachments = null)
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
            if (!IsValidEmail(recipient))
            {
                throw new ArgumentException("Dirección de correo inválida: " + recipient);
            }
        }

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_fromEmail, _appPassword);

        for (int i = 0; i < 2; i++)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(MailboxAddress.Parse(_fromEmail));
                message.Subject = subject;

                var multipart = new Multipart("mixed");
                string htmlBody = _htmlTemplate.Replace("{{subject}}", subject).Replace("{{body}}", body);
                multipart.Add(new TextPart(TextFormat.Html) { Text = htmlBody });

                if (attachments != null && attachments.Any())
                {
                    foreach (var file in attachments)
                    {
                        var memoryStream = new MemoryStream();
                        await file.CopyToAsync(memoryStream);
                        memoryStream.Position = 0;
                        var attachment = new MimePart(file.ContentType)
                        {
                            Content = new MimeContent(memoryStream),
                            ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                            ContentTransferEncoding = ContentEncoding.Base64,
                            FileName = file.FileName
                        };
                        multipart.Add(attachment);
                    }
                }
                message.Body = multipart;

                foreach (var recipient in recipients)
                {
                    message.To.Clear();
                    message.To.Add(MailboxAddress.Parse(recipient));
                    await smtp.SendAsync(message);

                    await SaveEmailToDatabase(recipient, subject, body);
                }

                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al enviar (intento {i + 1}): {ex.Message}");
                if (i == 1) throw;
            }
        }
        await smtp.DisconnectAsync(true);
    }

    private async Task SaveEmailToDatabase(string to, string subject, string body)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new NpgsqlCommand(
            "INSERT INTO \"GuardadoInformacion\" (\"Destinatario\", \"Asunto\", \"Contenido\") VALUES (@to, @subject, @body)",
            connection);

        command.Parameters.AddWithValue("to", to);
        command.Parameters.AddWithValue("subject", subject);
        command.Parameters.AddWithValue("body", body);

        await command.ExecuteNonQueryAsync();
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            MailboxAddress.Parse(email);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
