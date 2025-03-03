using Microsoft.Extensions.Logging;
using Moq;
using Supplier.Auth.Dto.Requests;
using Supplier.Auth.Dto.Responses;
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
        private readonly Mock<IToken> _tokenMock;
        private readonly Mock<ILogger<AuthService>> _loggerMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _tokenMock = new Mock<IToken>();
            _loggerMock = new Mock<ILogger<AuthService>>();
            _authService = new AuthService(_userRepositoryMock.Object, _tokenMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task RegisterUser_UserAlreadyExists_ReturnsErrorResponse()
        {
            // Arrange
            var request = new RegisterRequestDto
            {
                Email = "test@example.com",
                Password = "Password123"
            };
            _userRepositoryMock.Setup(repo => repo.UserExists(request.Email)).ReturnsAsync(true);

            // Act
            var result = await _authService.RegisterUser(request);

            // Assert
            Assert.Equal(Guid.Empty, result.UserId);
            Assert.Equal("User already registered.", result.Message);
        }

        [Fact]
        public async Task RegisterUser_CreateUserFails_ReturnsErrorResponse()
        {
            // Arrange
            var request = new RegisterRequestDto
            {
                Email = "test@example.com",
                Password = "Password123"
            };
            _userRepositoryMock.Setup(repo => repo.UserExists(request.Email)).ReturnsAsync(false);
            _userRepositoryMock.Setup(repo => repo.CreateUser(request.Email, request.Password)).ReturnsAsync((Guid?)null);

            // Act
            var result = await _authService.RegisterUser(request);

            // Assert
            Assert.Equal(Guid.Empty, result.UserId);
            Assert.Equal("Error creating user.", result.Message);
        }

        [Fact]
        public async Task RegisterUser_SuccessfulRegistration_ReturnsSuccessResponse()
        {
            // Arrange
            var request = new RegisterRequestDto
            {
                Email = "test@example.com",
                Password = "Password123"
            };
            var userId = Guid.NewGuid();
            _userRepositoryMock.Setup(repo => repo.UserExists(request.Email)).ReturnsAsync(false);
            _userRepositoryMock.Setup(repo => repo.CreateUser(request.Email, request.Password)).ReturnsAsync(userId);
            _userRepositoryMock.Setup(repo => repo.AssignRolesToUser(userId, request.Roles)).Returns(Task.CompletedTask);

            // Act
            var result = await _authService.RegisterUser(request);

            // Assert
            Assert.Equal(userId, result.UserId);
            Assert.Equal("User successfully registered.", result.Message);
        }

        [Fact]
        public async Task AuthenticateUser_InvalidCredentials_ReturnsErrorMessage()
        {
            // Arrange
            var request = new LoginRequestDto
            {
                Email = "invalid@example.com",
                Password = "WrongPassword"
            };
            _userRepositoryMock.Setup(repo => repo.GetUserByEmail(request.Email)).ReturnsAsync((User?)null);
            _userRepositoryMock.Setup(repo => repo.VerifyPassword(request.Email, request.Password)).ReturnsAsync(false);

            // Act
            var result = await _authService.AuthenticateUser(request);

            // Assert
            Assert.Null(result.Token);
            Assert.Equal("Invalid email or password.", result.Message);
        }

        [Fact]
        public async Task AuthenticateUser_ErrorRetrievingRoles_ReturnsErrorMessage()
        {
            // Arrange
            var request = new LoginRequestDto
            {
                Email = "test@example.com",
                Password = "Password123"
            };
            var user = new User { Id = Guid.NewGuid(), Email = request.Email, PasswordHash = "hashed_password" };
            _userRepositoryMock.Setup(repo => repo.GetUserByEmail(request.Email)).ReturnsAsync(user);
            _userRepositoryMock.Setup(repo => repo.VerifyPassword(request.Email, request.Password)).ReturnsAsync(true);
            _userRepositoryMock.Setup(repo => repo.GetUserRoles(user.Id)).ReturnsAsync((IEnumerable<string>?)null);

            // Act
            var result = await _authService.AuthenticateUser(request);

            // Assert
            Assert.Null(result.Token);
            Assert.Equal("Error retrieving user roles.", result.Message);
        }

        [Fact]
        public async Task AuthenticateUser_SuccessfulAuthentication_ReturnsToken()
        {
            // Arrange
            var request = new LoginRequestDto
            {
                Email = "test@example.com",
                Password = "Password123"
            };
            var user = new User { Id = Guid.NewGuid(), Email = request.Email, PasswordHash = "hashed_password" };
            var roles = new List<string> { "user" };
            var token = "valid.jwt.token";
            _userRepositoryMock.Setup(repo => repo.GetUserByEmail(request.Email)).ReturnsAsync(user);
            _userRepositoryMock.Setup(repo => repo.VerifyPassword(request.Email, request.Password)).ReturnsAsync(true);
            _userRepositoryMock.Setup(repo => repo.GetUserRoles(user.Id)).ReturnsAsync(roles);
            _tokenMock.Setup(jwt => jwt.GenerateToken(user.Id, user.Email, roles)).Returns(token);

            // Act
            var result = await _authService.AuthenticateUser(request);

            // Assert
            Assert.Equal(token, result.Token);
            Assert.Null(result.Message);
        }

        [Fact]
        public async Task RegisterAdminUser_UnauthenticatedUser_ReturnsErrorResponse()
        {
            // Arrange
            var request = new RegisterAdminRequestDto
            {
                Email = "admin@example.com",
                Password = "AdminPassword123"
            };
            var claims = new List<Claim>();
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);

            // Act
            var result = await _authService.RegisterAdminUser(request, user);

            // Assert
            Assert.Equal(Guid.Empty, result.UserId);
            Assert.Equal("Current user is not authenticated.", result.Message);
        }

        [Fact]
        public async Task RegisterAdminUser_NonAdminUser_ReturnsErrorResponse()
        {
            // Arrange
            var request = new RegisterAdminRequestDto
            {
                Email = "admin@example.com",
                Password = "AdminPassword123"
            };
            var adminId = Guid.NewGuid();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, adminId.ToString())
            };
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);

            _userRepositoryMock.Setup(repo => repo.IsUserInRole(adminId, "admin")).ReturnsAsync(false);

            // Act
            var result = await _authService.RegisterAdminUser(request, user);

            // Assert
            Assert.Equal(Guid.Empty, result.UserId);
            Assert.Equal("Only administrators can create other administrators.", result.Message);
        }

        [Fact]
        public async Task RegisterAdminUser_UserAlreadyExists_ReturnsErrorResponse()
        {
            // Arrange
            var request = new RegisterAdminRequestDto
            {
                Email = "admin@example.com",
                Password = "AdminPassword123"
            };
            var adminId = Guid.NewGuid();
            var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, adminId.ToString())
                    };
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);

            _userRepositoryMock.Setup(repo => repo.IsUserInRole(adminId, "admin")).ReturnsAsync(true);
            _userRepositoryMock.Setup(repo => repo.UserExists(request.Email)).ReturnsAsync(true);

            // Act
            var result = await _authService.RegisterAdminUser(request, user);

            // Assert
            Assert.Equal(Guid.Empty, result.UserId);
            Assert.Equal("User already registered.", result.Message);
        }

        [Fact]
        public async Task RegisterAdminUser_CreateUserFails_ReturnsErrorResponse()
        {
            // Arrange
            var request = new RegisterAdminRequestDto
            {
                Email = "admin@example.com",
                Password = "AdminPassword123"
            };
            var adminId = Guid.NewGuid();
            var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, adminId.ToString())
                    };
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);

            _userRepositoryMock.Setup(repo => repo.IsUserInRole(adminId, "admin")).ReturnsAsync(true);
            _userRepositoryMock.Setup(repo => repo.UserExists(request.Email)).ReturnsAsync(false);
            _userRepositoryMock.Setup(repo => repo.CreateUser(request.Email, request.Password)).ReturnsAsync((Guid?)null);

            // Act
            var result = await _authService.RegisterAdminUser(request, user);

            // Assert
            Assert.Equal(Guid.Empty, result.UserId);
            Assert.Equal("Error creating user.", result.Message);
        }

        [Fact]
        public async Task RegisterAdminUser_SuccessfulRegistration_ReturnsSuccessResponse()
        {
            // Arrange
            var request = new RegisterAdminRequestDto
            {
                Email = "admin@example.com",
                Password = "AdminPassword123"
            };
            var adminId = Guid.NewGuid();
            var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, adminId.ToString())
                    };
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);
            var userId = Guid.NewGuid();

            _userRepositoryMock.Setup(repo => repo.IsUserInRole(adminId, "admin")).ReturnsAsync(true);
            _userRepositoryMock.Setup(repo => repo.UserExists(request.Email)).ReturnsAsync(false);
            _userRepositoryMock.Setup(repo => repo.CreateUser(request.Email, request.Password)).ReturnsAsync(userId);
            _userRepositoryMock.Setup(repo => repo.AssignRolesToUser(userId, It.Is<List<string>>(roles => roles.Contains("admin"))))
                               .Returns(Task.CompletedTask);

            // Act
            var result = await _authService.RegisterAdminUser(request, user);

            // Assert
            Assert.Equal(userId, result.UserId);
            Assert.Equal("Admin user successfully registered.", result.Message);
        }

        [Fact]
        public async Task GetAllUsers_UnauthenticatedUser_ReturnsEmptyList()
        {
            // Arrange
            var claims = new List<Claim>();
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);

            // Act
            var result = await _authService.GetAllUsers(user);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllUsers_NonAdminUser_ReturnsEmptyList()
        {
            // Arrange
            var adminId = Guid.NewGuid();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, adminId.ToString())
            };
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);

            _userRepositoryMock.Setup(repo => repo.IsUserInRole(adminId, "admin")).ReturnsAsync(false);

            // Act
            var result = await _authService.GetAllUsers(user);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllUsers_SuccessfulRetrieval_ReturnsUserList()
        {
            // Arrange
            var adminId = Guid.NewGuid();
            var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, adminId.ToString())
                    };
            var identity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(identity);
            var users = new List<UserResponseDto>
                    {
                        new UserResponseDto { Id = Guid.NewGuid(), Email = "user1@example.com" },
                        new UserResponseDto { Id = Guid.NewGuid(), Email = "user2@example.com" }
                    };

            _userRepositoryMock.Setup(repo => repo.IsUserInRole(adminId, "admin")).ReturnsAsync(true);
            _userRepositoryMock.Setup(repo => repo.GetAllUsers()).ReturnsAsync(users);

            // Act
            var result = await _authService.GetAllUsers(user);

            // Assert
            Assert.Equal(2, ((List<UserResponseDto>)result).Count);
        }
    }
}
