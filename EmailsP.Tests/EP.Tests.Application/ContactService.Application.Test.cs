using Application.DTOs;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using Moq;
using Xunit;

namespace EP.Tests.Application
{
    public class ContactServiceTests
    {
        private readonly Mock<IContactRepository> _mockRepo;
        private readonly ContactService _service;

        public ContactServiceTests()
        {
            _mockRepo = new Mock<IContactRepository>();
            _service = new ContactService(_mockRepo.Object);
        }

        [Fact]
        public async Task CreateAsync_DeberiaCrearContacto_CuandoEmailNoExiste()
        {
            // Arrange
            var req = new CreateContactRequest
            {
                Name = "Juan Pérez",
                Email = "juan@test.com",
                Phone = "123456",
                Notes = "Cliente nuevo",
                IsFavorite = true,
                IsBlocked = false
            };

            _mockRepo.Setup(r => r.EmailExistsAsync(req.Email, null, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(false);

            var saved = new Contact
            {
                Id = Guid.NewGuid(),
                Name = req.Name,
                Email = req.Email,
                Phone = req.Phone,
                Notes = req.Notes,
                IsFavorite = req.IsFavorite,
                IsBlocked = req.IsBlocked,
                CreatedAt = DateTime.UtcNow
            };

            _mockRepo.Setup(r => r.AddAsync(It.IsAny<Contact>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(saved);

            // Act
            var result = await _service.CreateAsync(req);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(req.Email, result.Email);
            _mockRepo.Verify(r => r.AddAsync(It.IsAny<Contact>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_DeberiaLanzarExcepcion_CuandoEmailYaExiste()
        {
            var req = new CreateContactRequest { Email = "existente@test.com", Name = "Ana" };

            _mockRepo.Setup(r => r.EmailExistsAsync(req.Email, null, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(true);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(req));
        }

        [Fact]
        public async Task GetAsync_DeberiaRetornarNull_CuandoNoExiste()
        {
            var id = Guid.NewGuid();
            _mockRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync((Contact?)null);

            var result = await _service.GetAsync(id);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAsync_DeberiaRetornarContacto_CuandoExiste()
        {
            var id = Guid.NewGuid();
            var contact = new Contact { Id = id, Name = "Carlos", Email = "carlos@test.com" };

            _mockRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(contact);

            var result = await _service.GetAsync(id);

            Assert.NotNull(result);
            Assert.Equal("Carlos", result.Name);
        }
    }
}
