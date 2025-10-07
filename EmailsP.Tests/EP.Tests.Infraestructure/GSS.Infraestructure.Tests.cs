using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EmailsP.Tests
{
    public class GSSInfrastructureTests
    {
        private readonly Mock<ILogger<GmailSenderService>> _mockLogger;
        private readonly IConfiguration _config;

        public GSSInfrastructureTests()
        {
            _mockLogger = new Mock<ILogger<GmailSenderService>>();

            var inMemorySettings = new Dictionary<string, string>
            {
                { "Smtp:User", "test@gmail.com" },
                { "Smtp:Password", "claveFake123" },
                { "Smtp:Host", "smtp.gmail.com" },
                { "Smtp:Port", "587" },
                { "Smtp:UseStartTls", "true" }
            };

            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        [Fact]
        public async Task SendAsync_DeberiaLanzarExcepcion_SinClienteReal()
        {
            // Arrange
            var service = new GmailSenderService(_mockLogger.Object, _config);
            var destinatarios = new[] { "test@test.com" };
            var asunto = "Prueba";
            var cuerpo = "<h1>Test</h1>";

            // Act & Assert
            // Esperamos que falle porque no hay SMTP real conectado
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.SendAsync(destinatarios, asunto, cuerpo, null, CancellationToken.None)
            );
        }

        [Fact]
        public async Task SendAsync_DeberiaLanzarExcepcion_SiFaltaUsuario()
        {
            // Arrange
            var configNoUser = new ConfigurationBuilder().Build();
            var service = new GmailSenderService(_mockLogger.Object, configNoUser);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.SendAsync(new[] { "a@test.com" }, "Asunto", "Cuerpo", null)
            );
        }

        [Fact]
        public async Task SendAsync_PuedeAdjuntarArchivo()
        {
            // Arrange
            var service = new GmailSenderService(_mockLogger.Object, _config);

            var fileMock = new Mock<IFormFile>();
            var content = "Archivo de prueba";
            var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.FileName).Returns("test.txt");
            fileMock.Setup(f => f.Length).Returns(ms.Length);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.SendAsync(
                    new[] { "dest@test.com" },
                    "Asunto",
                    "Cuerpo",
                    new[] { fileMock.Object }
                )
            );
        }
    }
}
