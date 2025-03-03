using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Supplier.Auth.Configuration.Interfaces;
using Supplier.Auth.Dto.Responses;
using Supplier.Auth.Models;
using Supplier.Auth.Repositories;
using System.Data;

namespace Supplier.Auth.Tests.Repositories
{
    public class UserRepositoryTests
    {
        private readonly Mock<IDbConnectionFactory> _dbConnectionFactoryMock;
        private readonly Mock<IPasswordHasher<IdentityUser>> _passwordHasherMock;
        private readonly Mock<IDapperWrapper> _dapperWrapperMock;
        private readonly Mock<ILogger<UserRepository>> _loggerMock;
        private readonly UserRepository _userRepository;

        public UserRepositoryTests()
        {
            _dbConnectionFactoryMock = new Mock<IDbConnectionFactory>();
            _passwordHasherMock = new Mock<IPasswordHasher<IdentityUser>>();
            _dapperWrapperMock = new Mock<IDapperWrapper>();
            _loggerMock = new Mock<ILogger<UserRepository>>();
            _userRepository = new UserRepository(
                _dbConnectionFactoryMock.Object,
                _passwordHasherMock.Object,
                _dapperWrapperMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task CreateUser_UserDoesNotExist_ShouldCreateUser()
        {
            // Arrange
            var email = "test@example.com";
            var password = "password";
            var hashedPassword = "hashedPassword";
            var connectionMock = new Mock<IDbConnection>();

            _dbConnectionFactoryMock.Setup(f => f.CreateConnection()).Returns(connectionMock.Object);
            _dapperWrapperMock.Setup(d => d.ExecuteScalarAsync<int>(It.IsAny<IDbConnection>(), It.IsAny<CommandDefinition>()))
                .ReturnsAsync(0); // User does not exist
            _passwordHasherMock.Setup(p => p.HashPassword(It.IsAny<IdentityUser>(), password)).Returns(hashedPassword);

            // Act
            var result = await _userRepository.CreateUser(email, password);

            // Assert
            Assert.NotEqual(Guid.Empty, result);
        }

        [Fact]
        public async Task CreateUser_UserExists_ShouldReturnNull()
        {
            // Arrange
            var email = "test@example.com";
            var password = "password";
            var connectionMock = new Mock<IDbConnection>();

            _dbConnectionFactoryMock.Setup(f => f.CreateConnection()).Returns(connectionMock.Object);
            _dapperWrapperMock.Setup(d => d.ExecuteScalarAsync<int>(It.IsAny<IDbConnection>(), It.IsAny<CommandDefinition>()))
                .ReturnsAsync(1); // User exists

            // Act
            var result = await _userRepository.CreateUser(email, password);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateUser_ConnectionFails_ShouldReturnNull()
        {
            // Arrange
            var email = "test@example.com";
            var password = "password";

            _dbConnectionFactoryMock.Setup(f => f.CreateConnection()).Returns((IDbConnection?)null);

            // Act
            var result = await _userRepository.CreateUser(email, password);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UserExists_UserExists_ShouldReturnTrue()
        {
            // Arrange
            var email = "test@example.com";
            var connectionMock = new Mock<IDbConnection>();

            _dbConnectionFactoryMock.Setup(f => f.CreateConnection()).Returns(connectionMock.Object);
            _dapperWrapperMock.Setup(d => d.ExecuteScalarAsync<int>(It.IsAny<IDbConnection>(), It.IsAny<CommandDefinition>()))
                .ReturnsAsync(1); // User exists

            // Act
            var result = await _userRepository.UserExists(email);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task UserExists_UserDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var email = "test@example.com";
            var connectionMock = new Mock<IDbConnection>();

            _dbConnectionFactoryMock.Setup(f => f.CreateConnection()).Returns(connectionMock.Object);
            _dapperWrapperMock.Setup(d => d.ExecuteScalarAsync<int>(It.IsAny<IDbConnection>(), It.IsAny<CommandDefinition>()))
                .ReturnsAsync(0); // User does not exist

            // Act
            var result = await _userRepository.UserExists(email);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UserExists_ConnectionFails_ShouldReturnNull()
        {
            // Arrange
            var email = "test@example.com";

            _dbConnectionFactoryMock.Setup(f => f.CreateConnection()).Returns((IDbConnection?)null);

            // Act
            var result = await _userRepository.UserExists(email);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserByEmail_UserFound_ShouldReturnUser()
        {
            // Arrange
            var email = "test@example.com";
            var user = new User { Id = Guid.NewGuid(), Email = email, PasswordHash = "hashedPassword" };
            var connectionMock = new Mock<IDbConnection>();

            _dbConnectionFactoryMock.Setup(f => f.CreateConnection()).Returns(connectionMock.Object);
            _dapperWrapperMock.Setup(d => d.QueryFirstOrDefaultAsync<User>(It.IsAny<IDbConnection>(), It.IsAny<CommandDefinition>()))
                .ReturnsAsync(user);

            // Act
            var result = await _userRepository.GetUserByEmail(email);

            // Assert
            Assert.Equal(user, result);
        }

        [Fact]
        public async Task GetUserByEmail_UserNotFound_ShouldReturnNull()
        {
            // Arrange
            var email = "test@example.com";
            var connectionMock = new Mock<IDbConnection>();

            _dbConnectionFactoryMock.Setup(f => f.CreateConnection()).Returns(connectionMock.Object);
            _dapperWrapperMock.Setup(d => d.QueryFirstOrDefaultAsync<User>(It.IsAny<IDbConnection>(), It.IsAny<CommandDefinition>()))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userRepository.GetUserByEmail(email);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserByEmail_ConnectionFails_ShouldReturnNull()
        {
            // Arrange
            var email = "test@example.com";

            _dbConnectionFactoryMock.Setup(f => f.CreateConnection()).Returns((IDbConnection?)null);

            // Act
            var result = await _userRepository.GetUserByEmail(email);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserRoles_RolesFound_ShouldReturnRoles()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var roles = new List<string> { "Admin", "User" };
            var connectionMock = new Mock<IDbConnection>();

            _dbConnectionFactoryMock.Setup(f => f.CreateConnection()).Returns(connectionMock.Object);
            _dapperWrapperMock.Setup(d => d.QueryAsync<string>(It.IsAny<IDbConnection>(), It.IsAny<CommandDefinition>()))
                .ReturnsAsync(roles);

            // Act
            var result = await _userRepository.GetUserRoles(userId);

            // Assert
            Assert.Equal(roles, result);
        }

        [Fact]
        public async Task GetUserRoles_NoRolesFound_ShouldReturnEmptyList()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var connectionMock = new Mock<IDbConnection>();

            _dbConnectionFactoryMock.Setup(f => f.CreateConnection()).Returns(connectionMock.Object);
            _dapperWrapperMock.Setup(d => d.QueryAsync<string>(It.IsAny<IDbConnection>(), It.IsAny<CommandDefinition>()))
                .ReturnsAsync(Enumerable.Empty<string>());

            // Act
            var result = await _userRepository.GetUserRoles(userId);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetUserRoles_ConnectionFails_ShouldReturnNull()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _dbConnectionFactoryMock.Setup(f => f.CreateConnection()).Returns((IDbConnection?)null);

            // Act
            var result = await _userRepository.GetUserRoles(userId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task VerifyPassword_CorrectPassword_ShouldReturnTrue()
        {
            // Arrange
            var email = "test@example.com";
            var password = "password";
            var user = new User { Id = Guid.NewGuid(), Email = email, PasswordHash = "hashedPassword" };
            var connectionMock = new Mock<IDbConnection>();

            _dbConnectionFactoryMock.Setup(f => f.CreateConnection()).Returns(connectionMock.Object);
            _dapperWrapperMock.Setup(d => d.QueryFirstOrDefaultAsync<User>(It.IsAny<IDbConnection>(), It.IsAny<CommandDefinition>()))
                .ReturnsAsync(user);
            _passwordHasherMock.Setup(p => p.VerifyHashedPassword(It.IsAny<IdentityUser>(), user.PasswordHash, password))
                .Returns(PasswordVerificationResult.Success);

            // Act
            var result = await _userRepository.VerifyPassword(email, password);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task VerifyPassword_IncorrectPassword_ShouldReturnFalse()
        {
            // Arrange
            var email = "test@example.com";
            var password = "password";
            var user = new User { Id = Guid.NewGuid(), Email = email, PasswordHash = "hashedPassword" };
            var connectionMock = new Mock<IDbConnection>();

            _dbConnectionFactoryMock.Setup(f => f.CreateConnection()).Returns(connectionMock.Object);
            _dapperWrapperMock.Setup(d => d.QueryFirstOrDefaultAsync<User>(It.IsAny<IDbConnection>(), It.IsAny<CommandDefinition>()))
                .ReturnsAsync(user);
            _passwordHasherMock.Setup(p => p.VerifyHashedPassword(It.IsAny<IdentityUser>(), user.PasswordHash, password))
                .Returns(PasswordVerificationResult.Failed);

            // Act
            var result = await _userRepository.VerifyPassword(email, password);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task AssignRolesToUser_ShouldAssignRoles()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var roles = new List<string> { "Admin", "User" };
            var connectionMock = new Mock<IDbConnection>();

            _dbConnectionFactoryMock.Setup(f => f.CreateConnection()).Returns(connectionMock.Object);
            _dapperWrapperMock.Setup(d => d.QueryAsync<Role>(It.IsAny<IDbConnection>(), It.IsAny<CommandDefinition>()))
                .ReturnsAsync(Enumerable.Empty<Role>());

            // Act
            await _userRepository.AssignRolesToUser(userId, roles);

            // Assert
            _dapperWrapperMock.Verify(d => d.ExecuteAsync(It.IsAny<IDbConnection>(), It.IsAny<CommandDefinition>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task AssignRolesToUser_ConnectionFails_ShouldThrowException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var roles = new List<string> { "Admin", "User" };

            _dbConnectionFactoryMock.Setup(f => f.CreateConnection()).Returns((IDbConnection?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _userRepository.AssignRolesToUser(userId, roles));
        }

        [Fact]
        public async Task GetAllUsers_ShouldReturnUsers()
        {
            // Arrange
            var users = new List<UserResponseDto>
            {
                new UserResponseDto { Id = Guid.NewGuid(), Email = "user1@example.com" },
                new UserResponseDto { Id = Guid.NewGuid(), Email = "user2@example.com" }
            };
            var connectionMock = new Mock<IDbConnection>();

            _dbConnectionFactoryMock.Setup(f => f.CreateConnection()).Returns(connectionMock.Object);
            _dapperWrapperMock.Setup(d => d.QueryAsync<UserResponseDto>(It.IsAny<IDbConnection>(), It.IsAny<CommandDefinition>()))
                .ReturnsAsync(users);

            // Act
            var result = await _userRepository.GetAllUsers();

            // Assert
            Assert.Equal(users, result);
        }

        [Fact]
        public async Task GetAllUsers_NoUsersFound_ShouldReturnEmptyList()
        {
            // Arrange
            var connectionMock = new Mock<IDbConnection>();

            _dbConnectionFactoryMock.Setup(f => f.CreateConnection()).Returns(connectionMock.Object);
            _dapperWrapperMock.Setup(d => d.QueryAsync<UserResponseDto>(It.IsAny<IDbConnection>(), It.IsAny<CommandDefinition>()))
                .ReturnsAsync(Enumerable.Empty<UserResponseDto>());

            // Act
            var result = await _userRepository.GetAllUsers();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllUsers_ConnectionFails_ShouldReturnEmptyList()
        {
            // Arrange
            _dbConnectionFactoryMock.Setup(f => f.CreateConnection()).Returns((IDbConnection?)null);

            // Act
            var result = await _userRepository.GetAllUsers();

            // Assert
            Assert.Empty(result);
        }

    }
}
