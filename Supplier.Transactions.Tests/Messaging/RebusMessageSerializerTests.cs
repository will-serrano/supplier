using Microsoft.Extensions.Logging;
using Moq;
using Rebus.Messages;
using Supplier.Contracts.Transactions;
using Supplier.Contracts.Transactions.Requests;
using Supplier.Transactions.Messaging;
using System.Text;
using System.Text.Json;

namespace Supplier.Transactions.Tests.Messaging
{
    public class RebusMessageSerializerTests
    {
        private readonly Mock<ILogger<RebusMessageSerializer>> _loggerMock;
        private readonly RebusMessageSerializer _serializer;

        public RebusMessageSerializerTests()
        {
            _loggerMock = new Mock<ILogger<RebusMessageSerializer>>();
            _serializer = new RebusMessageSerializer(_loggerMock.Object);
        }

        [Fact]
        public async Task Serialize_NullMessage_ThrowsArgumentNullException()
        {
            // Arrange
            Message message = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _serializer.Serialize(message));
        }

        [Fact]
        public async Task Serialize_InvalidMessageBody_ThrowsInvalidOperationException()
        {
            // Arrange
            var message = new Message(new Dictionary<string, string>(), new object());

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _serializer.Serialize(message));
        }

        [Fact]
        public async Task Serialize_ValidMessage_ReturnsTransportMessage()
        {
            // Arrange
            var messageWrapper = new MessageWrapper
            {
                Data = new TransactionRequestMessageData()
            };
            var message = new Message(new Dictionary<string, string>(), messageWrapper);

            // Act
            var result = await _serializer.Serialize(message);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<TransportMessage>(result);
        }

        [Fact]
        public async Task Deserialize_NullTransportMessage_ThrowsArgumentNullException()
        {
            // Arrange
            TransportMessage transportMessage = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _serializer.Deserialize(transportMessage));
        }

        [Fact]
        public async Task Deserialize_InvalidJson_ThrowsInvalidOperationException()
        {
            // Arrange
            var transportMessage = new TransportMessage(new Dictionary<string, string>(), Encoding.UTF8.GetBytes("invalid json"));

            // Act & Assert
            await Assert.ThrowsAsync<JsonException>(() => _serializer.Deserialize(transportMessage));
        }

        [Fact]
        public async Task Deserialize_ValidTransportMessage_ReturnsMessage()
        {
            // Arrange
            var messageWrapper = new MessageWrapper
            {
                Data = new TransactionRequestMessageData()
            };
            var json = JsonSerializer.Serialize(messageWrapper);
            var transportMessage = new TransportMessage(new Dictionary<string, string>(), Encoding.UTF8.GetBytes(json));

            // Act
            var result = await _serializer.Deserialize(transportMessage);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Message>(result);
        }
    }
}
