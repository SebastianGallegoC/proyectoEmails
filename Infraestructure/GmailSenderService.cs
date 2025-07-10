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

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            // Leer plantilla HTML
            string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "EmailTemplate.html");
            
            //string htmlTemplate = await File.ReadAllTextAsync(templatePath);
            string htmlTemplate = @"
<!DOCTYPE html>
<html lang=""es"">
<head>
    <meta charset=""UTF-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{{subject}}</title>
    <style type=""text/css"">
        /* Client-specific Styles */
        body, table, td, a {
            -webkit-text-size-adjust: 100%;
            -ms-text-size-adjust: 100%;
        }

        table, td {
            mso-table-lspace: 0pt;
            mso-table-rspace: 0pt;
        }

        img {
            -ms-interpolation-mode: bicubic;
        }

        /* Resets */
        body {
            margin: 0;
            padding: 0;
            background-color: #f5f7fa; /* Un gris muy claro, como el de fondo de la imagen */
            font-family: 'Google Sans', 'Roboto', Arial, sans-serif; /* Fuentes que se acercan a Google/Gmail */
            color: #333333;
            -webkit-font-smoothing: antialiased;
            width: 100% !important;
        }

        p {
            margin: 0;
            padding: 0;
        }

        a {
            text-decoration: none;
        }

        img {
            border: 0;
            height: auto;
            line-height: 100%;
            outline: none;
            text-decoration: none;
            display: block;
        }

        table {
            border-collapse: collapse !important;
        }

        /* Mobile specific */
        @media only screen and (max-width: 600px) {
            .container {
                width: 100% !important;
                max-width: 100% !important;
            }

            .content-padding {
                padding: 20px !important;
            }

            .header-image img {
                width: 100% !important;
                height: auto !important;
            }

            .footer-links img {
                width: 24px !important; /* Ajuste para iconos de redes sociales en móvil */
                height: 24px !important;
            }

            .footer-section {
                padding: 10px 20px !important;
            }
        }
    </style>
</head>
<body style=""margin: 0; padding: 0; background-color: #f5f7fa; font-family: 'Google Sans', 'Roboto', Arial, sans-serif; color: #333333; -webkit-font-smoothing: antialiased;"">

    <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""border-collapse: collapse; mso-table-lspace: 0pt; mso-table-rspace: 0pt;"">
        <tr>
            <td align=""center"" style=""padding: 20px 0;"">
                <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""600"" class=""container"" style=""max-width: 600px; background-color: #ffffff; border-radius: 8px; box-shadow: 0 1px 3px rgba(0,0,0,0.12), 0 1px 2px rgba(0,0,0,0.24); overflow: hidden;"">
                    

                    <tr>
                        <td align=""center"" class=""header-image"" style=""background-color: #e80414; /* Color rojo de fondo si la imagen no llena o tiene transparencias */"">
                            <img src=""https://i.ytimg.com/vi/Rmmf0QP0X8g/maxresdefault.jpg"" alt=""Encabezado del Correo"" width=""600"" style=""display: block; border: 0; height: auto; line-height: 100%; outline: none; text-decoration: none; max-width: 100%; border-radius: 8px 8px 0 0;"">
                        </td>
                    </tr>

                    <tr>
                        <td class=""content-padding"" style=""padding: 30px; line-height: 1.6; font-family: Arial, sans-serif;"">
                            <p style=""font-size: 16px; margin-bottom: 20px; color: #333333;"">Hola:</p>
                            <p style=""font-size: 15px; margin-bottom: 15px; color: #424242;"">{{body}}</p>
                            <p style=""font-size: 15px; margin-bottom: 20px; color: #424242;"">Un afectuoso saludo,</p>
                            <p style=""font-size: 15px; color: #424242;"">[Tu Nombre]</p>
                        </td>
                    </tr>

                    <tr>
                        <td align=""center"" style=""padding: 10px 30px 20px 30px; font-family: Arial, sans-serif;"">
                        </td>
                    </tr>

                    <tr>
                        <td align=""center"" style=""background-color: #212121; padding: 30px 0; color: #bdbdbd; font-family: Arial, sans-serif; font-size: 12px;"">
                            

                            <p style=""margin: 0 0 10px 0; font-size: 12px; line-height: 18px;"">
                                Si tienes alguna duda, dirígete a La Fabrica De Software o revisa nuestras <a href=""#"" style=""color: #ffffff; text-decoration: underline;"">Preguntas Frecuentes</a>.
                            </p>
                            <p style=""margin: 0; font-size: 12px; line-height: 18px;"">
                                La Fabria De Software / FESC<br>
                                Cucuta, colombia, 540001
                            </p>
                            <p style=""margin-top: 15px; font-size: 11px; color: #9e9e9e;"">
                                Este mensaje fue generado automáticamente. Por favor, no responder.
                            </p>
                            <p style=""margin-top: 10px; font-size: 11px; color: #9e9e9e;"">
                                <a href=""#"" style=""color: #9e9e9e; text-decoration: underline;"">Política de Privacidad</a> | <a href=""#"" style=""color: #9e9e9e; text-decoration: underline;"">No volver a recibir estos emails</a>
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>
                ";

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
