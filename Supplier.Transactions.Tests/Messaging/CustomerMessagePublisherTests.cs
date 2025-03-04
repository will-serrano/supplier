using Microsoft.Extensions.Logging;
using Moq;
using Rebus.Bus;
using Rebus.Bus.Advanced;
using Supplier.Contracts.Transactions;
using Supplier.Contracts.Transactions.Enums;
using Supplier.Contracts.Transactions.Interfaces;
using Supplier.Contracts.Transactions.Requests;
using Supplier.Contracts.Transactions.Responses;
using Supplier.Transactions.Messaging;
using Supplier.Transactions.Messaging.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Supplier.Transactions.Tests.Messaging
{
    public class CustomerMessagePublisherTests
    {
        private readonly Mock<IBus> _busMock;
        private readonly Mock<IAdvancedApi> _advancedBusMock;
        private readonly Mock<IRoutingApi> _routingBusMock;
        private readonly Mock<ILogger<CustomerMessagePublisher>> _loggerMock;
        private readonly ICustomerMessagePublisher _publisher;

        public CustomerMessagePublisherTests()
        {
            _busMock = new Mock<IBus>();
            _advancedBusMock = new Mock<IAdvancedApi>();
            _routingBusMock = new Mock<IRoutingApi>();
            _loggerMock = new Mock<ILogger<CustomerMessagePublisher>>();

            _busMock.Setup(bus => bus.Advanced).Returns(_advancedBusMock.Object);
            _advancedBusMock.Setup(advanced => advanced.Routing).Returns(_routingBusMock.Object);

            _publisher = new CustomerMessagePublisher(_busMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Send_ValidMessage_LogsInformationAndSendsMessage()
        {
            // Arrange
            var message = new MessageWrapper
            {
                Version = "1.0",
                Data = new TransactionResponseMessageData // Use a concrete implementation
                {
                    TransactionId = Guid.NewGuid(),
                    IsSuccess = true,
                    NewLimit = 5000.0m,
                    Message = "Success"
                },
                Type = MessageType.TransactionRequestMessageData
            };

            // Act
            await _publisher.Send(message);

            // Assert
            _loggerMock.Verify(logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v != null && v.ToString().Contains("Serialized message:")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);

            _routingBusMock.Verify(routing => routing.Send(RoutingKeys.TransactionsToCustomers, message, null), Times.Once);
        }

        [Fact]
        public void Serialize_MessageWrapper_DetectsCycle()
        {
            // Arrange
            var message = new MessageWrapper
            {
                Version = "1.0",
                Data = new TransactionRequestMessageData // Use a concrete implementation
                {
                    Amount = 100.0m,
                    CustomerId = Guid.NewGuid(),
                    TransactionId = Guid.NewGuid()
                },
                Type = MessageType.TransactionRequestMessageData
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                ReferenceHandler = ReferenceHandler.Preserve,
                Converters = { new TransactionMessageDataConverter() }
            };

            // Act & Assert
            var exception = Record.Exception(() => JsonSerializer.Serialize(message, options));
            Assert.Null(exception);
        }
    }
}
