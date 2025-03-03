using Microsoft.Extensions.Logging;
using Moq;
using Supplier.Auth.Dto.Requests;
using Supplier.Auth.Models;
using Supplier.Auth.Repositories.Interfaces;
using Supplier.Auth.Services;
using Supplier.Auth.Services.Interfaces;
using System.Security.Claims;

namespace Supplier.Auth.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IToken> _jwtServiceMock;
        private readonly Mock<ILogger<AuthService>> _loggerMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _jwtServiceMock = new Mock<IToken>();
            _loggerMock = new Mock<ILogger<AuthService>>();
            _authService = new AuthService(_userRepositoryMock.Object, _jwtServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task RegisterUser_UserAlreadyExists_ReturnsErrorMessage()
        {
            // Arrange
            var request = new RegisterRequestDto { Email = "test@example.com", Password = "password" };
            _userRepositoryMock.Setup(repo => repo.UserExists(request.Email)).ReturnsAsync(true);

            // Act
            var result = await _authService.RegisterUser(request);

            // Assert
            Assert.Equal(Guid.Empty, result.UserId);
            Assert.Equal("User already registered.", result.Message);
        }

        [Fact]
        public async Task RegisterUser_UserCreationFails_ReturnsErrorMessage()
        {
            // Arrange
            var request = new RegisterRequestDto { Email = "test@example.com", Password = "password" };
            _userRepositoryMock.Setup(repo => repo.UserExists(request.Email)).ReturnsAsync(false);
            _userRepositoryMock.Setup(repo => repo.CreateUser(request.Email, request.Password)).ReturnsAsync((Guid?)null);

            // Act
            var result = await _authService.RegisterUser(request);

            // Assert
            Assert.Equal(Guid.Empty, result.UserId);
            Assert.Equal("Error creating user.", result.Message);
        }

        [Fact]
        public async Task RegisterUser_UserCreationSucceeds_ReturnsSuccessMessage()
        {
            // Arrange
            var request = new RegisterRequestDto { Email = "test@example.com", Password = "password" };
            var userId = Guid.NewGuid();
            _userRepositoryMock.Setup(repo => repo.UserExists(request.Email)).ReturnsAsync(false);
            _userRepositoryMock.Setup(repo => repo.CreateUser(request.Email, request.Password)).ReturnsAsync(userId);

            // Act
            var result = await _authService.RegisterUser(request);

            // Assert
            Assert.Equal(userId, result.UserId);
            Assert.Equal("User successfully registered.", result.Message);
        }

        [Fact]
        public async Task AuthenticateUser_InvalidEmailOrPassword_ReturnsErrorMessage()
        {
            // Arrange
            var request = new LoginRequestDto { Email = "test@example.com", Password = "password" };
            _userRepositoryMock.Setup(repo => repo.GetUserByEmail(request.Email)).ReturnsAsync((User)null);

            // Act
            var result = await _authService.AuthenticateUser(request);

            // Assert
            Assert.Equal("Invalid email or password.", result.Message);
        }

        [Fact]
        public async Task AuthenticateUser_ValidEmailAndPassword_ReturnsToken()
        {
            // Arrange
            var request = new LoginRequestDto { Email = "test@example.com", Password = "password" };
            var user = new User { Id = Guid.NewGuid(), Email = request.Email, PasswordHash = "hashed_password" };
            var roles = new List<string> { "user" };
            var token = "generated_token";

            _userRepositoryMock.Setup(repo => repo.GetUserByEmail(request.Email)).ReturnsAsync(user);
            _userRepositoryMock.Setup(repo => repo.VerifyPassword(request.Email, request.Password)).ReturnsAsync(true);
            _userRepositoryMock.Setup(repo => repo.GetUserRoles(user.Id)).ReturnsAsync(roles);
            _jwtServiceMock.Setup(service => service.GenerateToken(user.Id, user.Email, roles)).Returns(token);

            // Act
            var result = await _authService.AuthenticateUser(request);

            // Assert
            Assert.Equal(token, result.Token);
        }

        [Fact]
        public async Task RegisterAdminUser_NonAdminUser_ReturnsErrorMessage()
        {
            // Arrange
            var request = new RegisterAdminRequestDto { Email = "admin@example.com", Password = "password" };
            var currentUser = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Role, "user") }));

            // Act
            var result = await _authService.RegisterAdminUser(request, currentUser);

            // Assert
            Assert.Equal(Guid.Empty, result.UserId);
            Assert.Equal("Only administrators can create other administrators.", result.Message);
        }

        [Fact]
        public async Task RegisterAdminUser_AdminUser_ReturnsSuccessMessage()
        {
            // Arrange
            var request = new RegisterAdminRequestDto { Email = "admin@example.com", Password = "password" };
            var currentUser = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Role, "admin") }));
            var userId = Guid.NewGuid();

            _userRepositoryMock.Setup(repo => repo.UserExists(request.Email)).ReturnsAsync(false);
            _userRepositoryMock.Setup(repo => repo.CreateUser(request.Email, request.Password)).ReturnsAsync(userId);

            // Act
            var result = await _authService.RegisterAdminUser(request, currentUser);

            // Assert
            Assert.Equal(userId, result.UserId);
            Assert.Equal("Admin user successfully registered.", result.Message);
        }
    }
}
