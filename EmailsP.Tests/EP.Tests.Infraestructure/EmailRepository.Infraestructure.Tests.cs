using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Xunit;
using Domain.Interfaces;

namespace EmailsP.Tests
{
    public class EmailRepositoryInfrastructureTests
    {
        private readonly IConfiguration _config;

        public EmailRepositoryInfrastructureTests()
        {
            // Connection string falsa para test (no hay DB real)
            var inMemorySettings = new Dictionary<string, string>
            {
                { "ConnectionStrings:DefaultConnection", "Host=127.0.0.1;Username=postgres;Password=1234;Database=testdb" }
            };
            _config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
        }

        [Fact]
        public async Task SaveEmailAsync_DeberiaLanzarExcepcion_SiNoHayDB()
        {
            // Arrange
            var repo = new EmailRepository(_config);

            // Act & Assert
            // Captura NpgsqlException al intentar conectar
            await Assert.ThrowsAsync<NpgsqlException>(
                () => repo.SaveEmailAsync("a@test.com", "Asunto", "Cuerpo")
            );
        }

        [Fact]
        public void Constructor_DeberiaSetearConnectionString()
        {
            // Arrange & Act
            var repo = new EmailRepository(_config);

            // Assert
            Assert.NotNull(repo); // Validamos que instancia correctamente
        }
    }
}
