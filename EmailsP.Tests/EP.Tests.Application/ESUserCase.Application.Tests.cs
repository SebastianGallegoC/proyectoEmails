using Application.Services;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace EP.Tests.Application
{
    public class EmailSenderUseCaseTests
    {
        private readonly Mock<IEmailService> _mockSender;
        private readonly EmailSenderUseCase _useCase;

        public EmailSenderUseCaseTests()
        {
            _mockSender = new Mock<IEmailService>();
            _useCase = new EmailSenderUseCase(_mockSender.Object);
        }

        [Fact]
        public async Task ExecuteAsync_DeberiaLlamarSendAsyncConParametrosCorrectos()
        {
            // Arrange
            var destinatarios = new[] { "test1@mail.com", "test2@mail.com" };
            var asunto = "Prueba";
            var cuerpo = "Contenido del mensaje";
            IEnumerable<IFormFile>? adjuntos = null;

            _mockSender
                .Setup(s => s.SendAsync(destinatarios, asunto, cuerpo, adjuntos, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await _useCase.ExecuteAsync(destinatarios, asunto, cuerpo, adjuntos);

            // Assert
            _mockSender.Verify(
                s => s.SendAsync(destinatarios, asunto, cuerpo, adjuntos, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_DeberiaPropagarExcepcionDeEmailService()
        {
            var destinatarios = new[] { "error@test.com" };
            var asunto = "Falla";
            var cuerpo = "Mensaje";

            _mockSender
                .Setup(s => s.SendAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Error al enviar correo."));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _useCase.ExecuteAsync(destinatarios, asunto, cuerpo, null));
        }
    }
}
