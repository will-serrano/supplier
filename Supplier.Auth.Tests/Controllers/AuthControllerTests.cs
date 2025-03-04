using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Supplier.Auth.Controllers;
using Supplier.Auth.Dto.Requests;
using Supplier.Auth.Dto.Responses;
using Supplier.Auth.Services.Interfaces;

namespace Supplier.Auth.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly AuthController _authController;
        private readonly Mock<ILogger<AuthController>> _loggerMock;

        public AuthControllerTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _loggerMock = new Mock<ILogger<AuthController>>();
            _authController = new AuthController(_authServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Register_Success_ReturnsOk()
        {
            // Arrange
            var request = new RegisterRequestDto { Email = "test@example.com", Password = "Password123" };
            var response = new RegisterResponseDto(Guid.NewGuid(), "User registered successfully");
            _authServiceMock.Setup(s => s.RegisterUser(request)).ReturnsAsync(response);

            // Act
            var result = await _authController.Register(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task Register_Failure_ReturnsBadRequest()
        {
            // Arrange
            var request = new RegisterRequestDto { Email = "test@example.com", Password = "Password123" };
            var response = new RegisterResponseDto(Guid.Empty, "Registration failed");
            _authServiceMock.Setup(s => s.RegisterUser(request)).ReturnsAsync(response);

            // Act
            var result = await _authController.Register(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(response, badRequestResult.Value);
        }

        [Fact]
        public async Task Login_Success_ReturnsOk()
        {
            // Arrange
            var request = new LoginRequestDto { Email = "test@example.com", Password = "Password123" };
            var response = LoginResponseDto.WithToken("valid_token");
            _authServiceMock.Setup(s => s.AuthenticateUser(request)).ReturnsAsync(response);

            // Act
            var result = await _authController.Login(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task Login_Failure_ReturnsUnauthorized()
        {
            // Arrange
            var request = new LoginRequestDto { Email = "test@example.com", Password = "Password123" };
            var response = LoginResponseDto.WithMessage("Invalid credentials");
            _authServiceMock.Setup(s => s.AuthenticateUser(request)).ReturnsAsync(response);

            // Act
            var result = await _authController.Login(request);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(response, unauthorizedResult.Value);
        }
    }
}
