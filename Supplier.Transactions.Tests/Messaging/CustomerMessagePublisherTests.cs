using Microsoft.Extensions.Logging;
using Moq;
using Rebus.Bus;
using Rebus.Bus.Advanced;
using Supplier.Contracts.Transactions;
using Supplier.Contracts.Transactions.Interfaces;
using Supplier.Transactions.Messaging;

namespace Supplier.Transactions.Tests.Messaging
{
    public class CustomerMessagePublisherTests
    {
        private readonly Mock<IBus> _busMock;
        private readonly Mock<IAdvancedApi> _advancedBusMock;
        private readonly Mock<IRoutingApi> _routingBusMock;
        private readonly Mock<ILogger<CustomerMessagePublisher>> _loggerMock;
        private readonly CustomerMessagePublisher _publisher;

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
            var message = new MessageWrapper { Version = "1.0", Data = new Mock<ITransactionMessageData>().Object };

            // Act
            await _publisher.Send(message);

            // Assert
            _loggerMock.Verify(logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Sending message to the customer queue.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            _routingBusMock.Verify(routing => routing.Send(RoutingKeys.TransactionsToCustomers, message, null), Times.Once);
        }
    }
}
