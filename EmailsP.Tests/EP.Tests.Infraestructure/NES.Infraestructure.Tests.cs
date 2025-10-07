using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EmailsP.Tests
{
    public class NESInfrastructureTests
    {
        private readonly NullEmailService _service;
        private readonly Mock<ILogger<NullEmailService>> _mockLogger;

        public NESInfrastructureTests()
        {
            _mockLogger = new Mock<ILogger<NullEmailService>>();
            _service = new NullEmailService(_mockLogger.Object);
        }

        [Fact]
        public async Task SendAsync_DeberiaEjecutarseSinExcepciones_SinAdjuntos()
        {
            // Act & Assert
            await _service.SendAsync(new[] { "test@test.com" }, "Asunto", "Cuerpo", null, CancellationToken.None);
        }

        [Fact]
        public async Task SendAsync_DeberiaEjecutarseConAdjuntos()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var content = "Archivo de prueba";
            var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.FileName).Returns("test.txt");
            fileMock.Setup(f => f.Length).Returns(ms.Length);

            var attachments = new[] { fileMock.Object };

            // Act & Assert
            await _service.SendAsync(new[] { "a@test.com" }, "Asunto", "Cuerpo", attachments, CancellationToken.None);
        }

        [Fact]
        public async Task SendAsync_DeberiaRegistrarLog()
        {
            // Arrange
            var destinatarios = new[] { "a@test.com" };

            // Act
            await _service.SendAsync(destinatarios, "Asunto", "Cuerpo", null, CancellationToken.None);

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Simulación envío")),
                    It.IsAny<System.Exception>(),
                    It.IsAny<System.Func<It.IsAnyType, System.Exception?, string>>()),
                Times.Once
            );
        }
    }
}
