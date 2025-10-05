using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Domain.Interfaces;

using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace Infrastructure.Services
{
    public class GmailSenderService : IEmailService
    {
        private readonly ILogger<GmailSenderService> _logger;
        private readonly IConfiguration _cfg;

        public GmailSenderService(ILogger<GmailSenderService> logger, IConfiguration cfg)
        {
            _logger = logger;
            _cfg = cfg;
        }

        public async Task SendAsync(
            IEnumerable<string> to,
            string subject,
            string? bodyHtml,
            IEnumerable<IFormFile>? attachments,
            CancellationToken ct = default)
        {
            // -------- Config y validaciones --------
            var user = _cfg["Smtp:User"];
            if (string.IsNullOrWhiteSpace(user))
                throw new InvalidOperationException("SMTP: falta Smtp:User en configuración.");

            var host = _cfg["Smtp:Host"] ?? "smtp.gmail.com";
            var port = int.TryParse(_cfg["Smtp:Port"], out var p) ? p : 587;
            var useStartTls = bool.TryParse(_cfg["Smtp:UseStartTls"], out var st) ? st : true;
            var secure = useStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.SslOnConnect;

            // -------- Construcción del mensaje (todo en memoria) --------
            var msg = new MimeMessage();
            msg.From.Add(MailboxAddress.Parse(user));

            foreach (var addr in (to ?? Enumerable.Empty<string>())
                                 .Where(a => !string.IsNullOrWhiteSpace(a))
                                 .Distinct(StringComparer.OrdinalIgnoreCase))
                msg.To.Add(MailboxAddress.Parse(addr));

            msg.Subject = subject ?? string.Empty;

            var body = new BodyBuilder { HtmlBody = bodyHtml ?? string.Empty };
            if (attachments != null)
            {
                foreach (var file in attachments)
                {
                    if (file == null || file.Length == 0) continue;
                    await using var ms = new MemoryStream();
                    await file.CopyToAsync(ms, ct);
                    body.Attachments.Add(
                        string.IsNullOrWhiteSpace(file.FileName) ? "adjunto.bin" : Path.GetFileName(file.FileName),
                        ms.ToArray()
                    );
                }
            }
            msg.Body = body.ToMessageBody();

            using var client = new SmtpClient { Timeout = 120_000 };

            try
            {
                await client.ConnectAsync(host, port, secure, ct);
                await client.AuthenticateAsync(user, _cfg["Smtp:Password"], ct);
                await client.SendAsync(msg, ct);
            }
            // ===== Errores más comunes con mensajes útiles =====
            catch (SslHandshakeException ex)
            {
                _logger.LogError(ex, "TLS handshake falló con {Host}:{Port} (UseStartTls={UseStartTls})", host, port, useStartTls);
                throw new InvalidOperationException($"SMTP/TLS: fallo de handshake con {host}:{port}. Suele ser inspección SSL o versión TLS no soportada. Detalle: {ex.Message}", ex);
            }
            catch (MailKit.Security.AuthenticationException ex)
            {
                _logger.LogError(ex, "Autenticación SMTP inválida para {User}", user);
                throw new InvalidOperationException("SMTP: credenciales inválidas (usa App Password si es Gmail).", ex);
            }
            catch (SmtpCommandException ex)
            {
                _logger.LogError(ex, "SMTP command error {Status} en {Host}:{Port}", ex.StatusCode, host, port);
                throw new InvalidOperationException($"SMTP: comando falló ({ex.StatusCode}). Detalle: {ex.Message}", ex);
            }
            catch (SmtpProtocolException ex)
            {
                _logger.LogError(ex, "SMTP protocolo inválido en {Host}:{Port}", host, port);
                throw new InvalidOperationException($"SMTP: error de protocolo. Detalle: {ex.Message}", ex);
            }
            catch (SocketException ex)
            {
                _logger.LogError(ex, "No se pudo conectar a {Host}:{Port}", host, port);
                throw new InvalidOperationException($"SMTP: no se pudo conectar a {host}:{port} ({ex.SocketErrorCode}). ¿Firewall/puerto/bloqueo de red?", ex);
            }
            catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
            {
                _logger.LogError(ex, "Timeout al enviar SMTP {Host}:{Port}", host, port);
                throw new InvalidOperationException("SMTP: timeout de conexión/envío. Revisa red/puertos.", ex);
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "I/O durante envío SMTP");
                throw new InvalidOperationException($"SMTP: error de E/S durante el envío. Detalle: {ex.Message}", ex);
            }
            catch (ObjectDisposedException ex)
            {
                _logger.LogError(ex, "Conexión/stream dispuesto inesperadamente");
                throw new InvalidOperationException("SMTP: la conexión se cerró inesperadamente (inspección SSL / proxy / antivirus).", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado enviando correo");
                throw new InvalidOperationException($"SMTP inesperado: {ex.Message}", ex);
            }
            finally
            {
                try
                {
                    if (client.IsConnected) await client.DisconnectAsync(true, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Fallo al desconectar SMTP");
                }
            }
        }
    }
}
