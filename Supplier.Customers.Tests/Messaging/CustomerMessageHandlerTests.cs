using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Rebus.Bus;
using Rebus.Bus.Advanced;
using Supplier.Contracts.Transactions;
using Supplier.Contracts.Transactions.Interfaces;
using Supplier.Contracts.Transactions.Requests;
using Supplier.Customers.Messaging;
using Supplier.Customers.Models;
using Supplier.Customers.Repositories.Interfaces;

namespace Supplier.Customers.Tests.Messaging
{
    public class CustomerMessageHandlerTests
    {
        private readonly Mock<ICustomerRepository> _customerRepositoryMock;
        private readonly Mock<IBus> _busMock;
        private readonly Mock<ILogger<CustomerMessageHandler>> _loggerMock;
        private readonly Mock<IMemoryCache> _customerCacheMock;
        private readonly CustomerMessageHandler _handler;

        public CustomerMessageHandlerTests()
        {
            _customerRepositoryMock = new Mock<ICustomerRepository>();
            _busMock = new Mock<IBus>();
            _loggerMock = new Mock<ILogger<CustomerMessageHandler>>();
            _customerCacheMock = new Mock<IMemoryCache>();
            _handler = new CustomerMessageHandler(_customerRepositoryMock.Object, _busMock.Object, _loggerMock.Object, _customerCacheMock.Object);
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
                It.Is<It.IsAnyType>((v, t) => v != null && v.ToString()!.Contains("Message received with empty data.")),
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
                It.Is<It.IsAnyType>((v, t) => v != null && v.ToString()!.Contains("Failed to convert message to TransactionRequestMessageData.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task Handle_CustomerNotFound_LogsErrorAndSendsFailureResponse()
        {
            // Arrange
            var transactionData = new TransactionRequestMessageData { CustomerId = Guid.NewGuid(), TransactionId = Guid.NewGuid() };
            var message = new MessageWrapper { Data = transactionData };
            _customerRepositoryMock.Setup(repo => repo.GetCustomerByIdAsync(transactionData.CustomerId)).ReturnsAsync((Customer?)null);

            var advancedBusMock = new Mock<IAdvancedApi>();
            var routingBusMock = new Mock<IRoutingApi>();
            _busMock.Setup(bus => bus.Advanced).Returns(advancedBusMock.Object);
            _busMock.Setup(bus => bus.Advanced.Routing).Returns(routingBusMock.Object);

            // Act
            await _handler.Handle(message);

            // Assert
            _loggerMock.Verify(logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v != null && v.ToString()!.Contains($"Customer not found: {transactionData.CustomerId}")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);

            _busMock.Verify(bus => bus.Advanced.Routing.Send(It.IsAny<string>(), It.IsAny<MessageWrapper>(), null), Times.Once);
        }

        [Fact]
        public async Task Handle_CustomerCreditLimitUpdateFails_LogsErrorAndSendsFailureResponse()
        {
            // Arrange
            var transactionData = new TransactionRequestMessageData { CustomerId = Guid.NewGuid(), TransactionId = Guid.NewGuid(), Amount = 100 };
            var message = new MessageWrapper { Data = transactionData };
            var customer = new Customer { Id = transactionData.CustomerId, CreditLimit = 50 };
            _customerRepositoryMock.Setup(repo => repo.GetCustomerByIdAsync(transactionData.CustomerId)).ReturnsAsync(customer);
            customer.TryReduceCreditLimit(transactionData.Amount, out _);

            var advancedBusMock = new Mock<IAdvancedApi>();
            var routingBusMock = new Mock<IRoutingApi>();
            _busMock.Setup(bus => bus.Advanced).Returns(advancedBusMock.Object);
            _busMock.Setup(bus => bus.Advanced.Routing).Returns(routingBusMock.Object);

            // Act
            await _handler.Handle(message);

            // Assert
            _loggerMock.Verify(logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v != null && v.ToString()!.Contains($"Failed to update customer limit {transactionData.CustomerId}")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);
            advancedBusMock.Verify(bus => bus.Routing.Send(It.IsAny<string>(), It.IsAny<MessageWrapper>(), null), Times.Once);
        }

        [Fact]
        public async Task Handle_CustomerCreditLimitUpdateSucceeds_LogsInformationAndSendsSuccessResponse()
        {
            // Arrange
            var transactionData = new TransactionRequestMessageData { CustomerId = Guid.NewGuid(), TransactionId = Guid.NewGuid(), Amount = 100 };
            var message = new MessageWrapper { Data = transactionData };
            var customer = new Customer { Id = transactionData.CustomerId, CreditLimit = 200 };
            _customerRepositoryMock.Setup(repo => repo.GetCustomerByIdAsync(transactionData.CustomerId)).ReturnsAsync(customer);

            var advancedBusMock = new Mock<IAdvancedApi>();
            var routingBusMock = new Mock<IRoutingApi>();
            _busMock.Setup(bus => bus.Advanced).Returns(advancedBusMock.Object);
            _busMock.Setup(bus => bus.Advanced.Routing).Returns(routingBusMock.Object);

            // Act
            await _handler.Handle(message);

            // Assert
            _loggerMock.Verify(logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v != null && v.ToString()!.Contains($"Customer limit {customer.Id} updated to {customer.CreditLimit}")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);
            advancedBusMock.Verify(bus => bus.Routing.Send(It.IsAny<string>(), It.IsAny<MessageWrapper>(), null), Times.Once);
        }
    }
}
