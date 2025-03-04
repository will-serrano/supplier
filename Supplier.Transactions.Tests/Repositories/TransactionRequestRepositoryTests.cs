using Dapper;
using Microsoft.Extensions.Logging;
using Moq;
using Supplier.Transactions.Configuration.Interfaces;
using Supplier.Transactions.Enums;
using Supplier.Transactions.Models;
using Supplier.Transactions.Repositories;
using Supplier.Transactions.Repositories.Interfaces;
using System.Data;

namespace Supplier.Transactions.Tests.Repositories
{
    public class TransactionRequestRepositoryTests
    {
        private readonly Mock<IDbConnectionFactory> _mockDbConnectionFactory;
        private readonly Mock<IDapperWrapper> _mockDapperWrapper;
        private readonly Mock<ILogger<TransactionRequestRepository>> _mockLogger;
        private readonly TransactionRequestRepository _repository;

        public TransactionRequestRepositoryTests()
        {
            _mockDbConnectionFactory = new Mock<IDbConnectionFactory>();
            _mockDapperWrapper = new Mock<IDapperWrapper>();
            _mockLogger = new Mock<ILogger<TransactionRequestRepository>>();
            _repository = new TransactionRequestRepository(_mockDbConnectionFactory.Object, _mockDapperWrapper.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task RegisterTransactionRequestAsync_ShouldReturnTransaction_WhenSuccessful()
        {
            // Arrange
            var transaction = new TransactionRequest { Id = Guid.NewGuid(), CustomerId = Guid.NewGuid(), Amount = 100, CustomerBlocked = false };
            var connection = new Mock<IDbConnection>();
            _mockDbConnectionFactory.Setup(factory => factory.CreateConnection()).Returns(connection.Object);
            _mockDapperWrapper.Setup(dapper => dapper.QueryFirstOrDefaultAsync<TransactionRequest>(connection.Object, It.IsAny<CommandDefinition>()))
                .ReturnsAsync(transaction);

            // Act
            var result = await _repository.RegisterTransactionRequestAsync(transaction);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(transaction.Id, result?.Id);
        }

        [Fact]
        public async Task RegisterTransactionRequestAsync_ShouldReturnNull_WhenFailed()
        {
            // Arrange
            var transaction = new TransactionRequest { Id = Guid.NewGuid(), CustomerId = Guid.NewGuid(), Amount = 100, CustomerBlocked = false };
            var connection = new Mock<IDbConnection>();
            _mockDbConnectionFactory.Setup(factory => factory.CreateConnection()).Returns(connection.Object);
            _mockDapperWrapper.Setup(dapper => dapper.QueryFirstOrDefaultAsync<TransactionRequest>(connection.Object, It.IsAny<CommandDefinition>()))
                .ReturnsAsync((TransactionRequest?)null);

            // Act
            var result = await _repository.RegisterTransactionRequestAsync(transaction);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateTransactionRequestAsync_ShouldUpdateTransaction_WhenCalled()
        {
            // Arrange
            var transaction = new TransactionRequest { Id = Guid.NewGuid(), Amount = 200, Status = TransactionStatus.Processing, UpdatedBy = "User", Detail = "Updated" };
            var connection = new Mock<IDbConnection>();
            _mockDbConnectionFactory.Setup(factory => factory.CreateConnection()).Returns(connection.Object);
            _mockDapperWrapper.Setup(dapper => dapper.ExecuteAsync(connection.Object, It.IsAny<CommandDefinition>()))
                .ReturnsAsync(1);

            // Act
            await _repository.UpdateTransactionRequestAsync(transaction);

            // Assert
            _mockDapperWrapper.Verify(dapper => dapper.ExecuteAsync(connection.Object, It.IsAny<CommandDefinition>()), Times.Once);
        }

        [Fact]
        public async Task UpdateTransactionRequestAsync_ShouldUpdateTransactionStatusToCompleted_WhenCalled()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var connection = new Mock<IDbConnection>();
            _mockDbConnectionFactory.Setup(factory => factory.CreateConnection()).Returns(connection.Object);
            _mockDapperWrapper.Setup(dapper => dapper.ExecuteAsync(connection.Object, It.IsAny<CommandDefinition>()))
                .ReturnsAsync(1);

            // Act
            await _repository.UpdateTransactionRequestAsync(transactionId);

            // Assert
            _mockDapperWrapper.Verify(dapper => dapper.ExecuteAsync(connection.Object, It.IsAny<CommandDefinition>()), Times.Once);
        }

        [Fact]
        public async Task UpdateTransactionRequestAsync_ShouldUpdateTransactionStatusToFailed_WhenCalled()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var message = "Failure message";
            var connection = new Mock<IDbConnection>();
            _mockDbConnectionFactory.Setup(factory => factory.CreateConnection()).Returns(connection.Object);
            _mockDapperWrapper.Setup(dapper => dapper.ExecuteAsync(connection.Object, It.IsAny<CommandDefinition>()))
                .ReturnsAsync(1);

            // Act
            await _repository.UpdateTransactionRequestAsync(transactionId, message);

            // Assert
            _mockDapperWrapper.Verify(dapper => dapper.ExecuteAsync(connection.Object, It.IsAny<CommandDefinition>()), Times.Once);
        }
    }
}
