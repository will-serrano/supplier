using Microsoft.Extensions.Logging;
using Moq;
using Supplier.Contracts.Transactions;
using Supplier.Contracts.Transactions.Interfaces;
using Supplier.Contracts.Transactions.Responses;
using Supplier.Transactions.Messaging;
using Supplier.Transactions.Repositories.Interfaces;

namespace Supplier.Transactions.Tests.Messaging
{
    public class TransactionMessageHandlerTests
    {
        private readonly Mock<ITransactionRequestRepository> _transactionRequestRepositoryMock;
        private readonly Mock<ILogger<TransactionMessageHandler>> _loggerMock;
        private readonly TransactionMessageHandler _handler;

        public TransactionMessageHandlerTests()
        {
            _transactionRequestRepositoryMock = new Mock<ITransactionRequestRepository>();
            _loggerMock = new Mock<ILogger<TransactionMessageHandler>>();
            _handler = new TransactionMessageHandler(_transactionRequestRepositoryMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_MessageWithNullData_LogsError()
        {
            // Arrange
            var message = new MessageWrapper { Data = null };

            // Act
            await _handler.Handle(message);

            // Assert
            _loggerMock.Verify(logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v != null && v.ToString()!.Contains("Received message with empty data.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task Handle_MessageWithInvalidData_LogsError()
        {
            // Arrange
            var message = new MessageWrapper { Data = new Mock<ITransactionMessageData>().Object };

            // Act
            await _handler.Handle(message);

            // Assert
            _loggerMock.Verify(logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v != null && v.ToString()!.Contains("Failed to convert message to TransactionResponseMessageData.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task Handle_TransactionRejected_LogsErrorAndUpdatesTransaction()
        {
            // Arrange
            var transactionData = new TransactionResponseMessageData { IsSuccess = false, TransactionId = Guid.NewGuid(), Message = "Error message" };
            var message = new MessageWrapper { Data = transactionData };

            // Act
            await _handler.Handle(message);

            // Assert
            _loggerMock.Verify(logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v != null && v.ToString()!.Contains("Transaction rejected")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);

            _transactionRequestRepositoryMock.Verify(repo => repo.UpdateTransactionRequestAsync(transactionData.TransactionId, transactionData.Message), Times.Once);
        }

        [Fact]
        public async Task Handle_TransactionCompleted_LogsInformationAndUpdatesTransaction()
        {
            // Arrange
            var transactionData = new TransactionResponseMessageData { IsSuccess = true, TransactionId = Guid.NewGuid(), NewLimit = 1000 };
            var message = new MessageWrapper { Data = transactionData };

            // Act
            await _handler.Handle(message);

            // Assert
            _loggerMock.Verify(logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v != null && v.ToString()!.Contains("Transaction completed")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);

            _transactionRequestRepositoryMock.Verify(repo => repo.UpdateTransactionRequestAsync(transactionData.TransactionId), Times.Once);
        }
    }
}
