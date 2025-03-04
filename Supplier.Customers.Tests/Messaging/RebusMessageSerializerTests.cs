using Microsoft.Extensions.Logging;
using Moq;
using Rebus.Messages;
using Supplier.Contracts.Transactions;
using Supplier.Contracts.Transactions.Requests;
using Supplier.Customers.Messaging;
using System.Text;
using System.Text.Json;

namespace Supplier.Customers.Tests.Messaging
{
    /// <summary>
    /// Unit tests for the <see cref="RebusMessageSerializer"/> class.
    /// </summary>
    public class RebusMessageSerializerTests
    {
        private readonly Mock<ILogger<RebusMessageSerializer>> _loggerMock;
        private readonly RebusMessageSerializer _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="RebusMessageSerializerTests"/> class.
        /// </summary>
        public RebusMessageSerializerTests()
        {
            _loggerMock = new Mock<ILogger<RebusMessageSerializer>>();
            _serializer = new RebusMessageSerializer(_loggerMock.Object);
        }

        /// <summary>
        /// Tests that <see cref="RebusMessageSerializer.Serialize"/> throws <see cref="ArgumentNullException"/> when the message is null.
        /// </summary>
        [Fact]
        public async Task Serialize_NullMessage_ThrowsArgumentNullException()
        {
            // Arrange
            Message message = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _serializer.Serialize(message));
        }

        /// <summary>
        /// Tests that <see cref="RebusMessageSerializer.Serialize"/> throws <see cref="InvalidOperationException"/> when the message body is invalid.
        /// </summary>
        [Fact]
        public async Task Serialize_InvalidMessageBody_ThrowsInvalidOperationException()
        {
            // Arrange
            var message = new Message(new Dictionary<string, string>(), new object());

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _serializer.Serialize(message));
        }

        /// <summary>
        /// Tests that <see cref="RebusMessageSerializer.Serialize"/> returns a <see cref="TransportMessage"/> when the message is valid.
        /// </summary>
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

        /// <summary>
        /// Tests that <see cref="RebusMessageSerializer.Deserialize"/> throws <see cref="ArgumentNullException"/> when the transport message is null.
        /// </summary>
        [Fact]
        public async Task Deserialize_NullTransportMessage_ThrowsArgumentNullException()
        {
            // Arrange
            TransportMessage transportMessage = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _serializer.Deserialize(transportMessage));
        }

        /// <summary>
        /// Tests that <see cref="RebusMessageSerializer.Deserialize"/> throws <see cref="JsonException"/> when the JSON is invalid.
        /// </summary>
        [Fact]
        public async Task Deserialize_InvalidJson_ThrowsInvalidOperationException()
        {
            // Arrange
            var transportMessage = new TransportMessage(new Dictionary<string, string>(), Encoding.UTF8.GetBytes("invalid json"));

            // Act & Assert
            await Assert.ThrowsAsync<JsonException>(() => _serializer.Deserialize(transportMessage));
        }

        /// <summary>
        /// Tests that <see cref="RebusMessageSerializer.Deserialize"/> returns a <see cref="Message"/> when the transport message is valid.
        /// </summary>
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
