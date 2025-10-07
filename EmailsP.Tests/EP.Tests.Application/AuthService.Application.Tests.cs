using Application.DTOs;
using Application.Services;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Xunit;

namespace EP.Tests.Application
{
    public class AuthServiceTests
    {
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                {"Jwt:Key", "claveSuperSecretaDePrueba1234567890123456"},
                {"Jwt:Issuer", "testIssuer"},
                {"Jwt:Audience", "testAudience"}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _authService = new AuthService(configuration);
        }

        [Fact]
        public void Authenticate_DeberiaRetornarToken_CuandoCredencialesSonValidas()
        {
            var request = new LoginRequest
            {
                Username = "admin",
                Password = "1234"
            };

            var result = _authService.Authenticate(request);

            Assert.NotNull(result);
            Assert.False(string.IsNullOrWhiteSpace(result.Token));
            Assert.True(result.Expiration > DateTime.UtcNow);
        }

        [Fact]
        public void Authenticate_DeberiaRetornarNull_CuandoCredencialesSonInvalidas()
        {
            var request = new LoginRequest
            {
                Username = "user",
                Password = "wrong"
            };

            var result = _authService.Authenticate(request);

            Assert.Null(result);
        }
    }
}
