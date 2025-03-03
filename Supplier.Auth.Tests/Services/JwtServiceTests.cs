using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Supplier.Auth.Configuration;
using Supplier.Auth.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Supplier.Auth.Tests.Services
{
    public class JwtServiceTests
    {
        private readonly JwtService _jwtService;
        private readonly Mock<IOptions<JwtSettings>> _jwtSettingsMock;
        private readonly Mock<ILogger<JwtService>> _loggerMock;

        public JwtServiceTests()
        {
            _jwtSettingsMock = new Mock<IOptions<JwtSettings>>();
            _loggerMock = new Mock<ILogger<JwtService>>();
            _jwtSettingsMock.Setup(s => s.Value).Returns(new JwtSettings
            {
                Secret = "supersecretkey12345678901234567890", // Ensure the key is at least 32 characters long
                Issuer = "testIssuer",
                Audience = "testAudience",
                ExpirationMinutes = 60
            });

            _jwtService = new JwtService(_jwtSettingsMock.Object, _loggerMock.Object);
        }

        [Fact]
        public void GenerateToken_ValidInputs_ReturnsToken()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var email = "test@example.com";
            var roles = new List<string> { "Admin", "User" };

            // Act
            var token = _jwtService.GenerateToken(userId, email, roles);

            // Assert
            Assert.NotNull(token);
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            Assert.Equal(userId.ToString(), jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
            Assert.Equal(email, jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
            Assert.Contains(jwtToken.Claims, c => c.Type == "role" && c.Value == "Admin");
            Assert.Contains(jwtToken.Claims, c => c.Type == "role" && c.Value == "User");

        }

        [Fact]
        public void GenerateToken_EmptyRoles_ReturnsTokenWithoutRoles()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var email = "test@example.com";
            var roles = new List<string>();

            // Act
            var token = _jwtService.GenerateToken(userId, email, roles);

            // Assert
            Assert.NotNull(token);
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            Assert.Equal(userId.ToString(), jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
            Assert.Equal(email, jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
            Assert.DoesNotContain(jwtToken.Claims, c => c.Type == ClaimTypes.Role);
        }
    }
}
