using Rebus.Messages;
using Rebus.Serialization;
using Supplier.Contracts.Transactions;
using Supplier.Contracts.Transactions.Enums;
using Supplier.Contracts.Transactions.Requests;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Supplier.Customers.Messaging
{
    /// <summary>
    /// Serializer for Rebus messages using JSON.
    /// </summary>
    public class RebusMessageSerializer : ISerializer
    {
        private readonly JsonSerializerOptions _options;
        private readonly ILogger<RebusMessageSerializer> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RebusMessageSerializer"/> class.
        /// </summary>
        /// <param name="logger">The logger to use for logging.</param>
        public RebusMessageSerializer(ILogger<RebusMessageSerializer> logger)
        {
            _options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = false,
                Converters = { new TransactionMessageDataConverter() } // Adiciona o conversor customizado
            };
            _logger = logger;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RebusMessageSerializer"/> class with custom options.
        /// </summary>
        /// <param name="options">The JSON serializer options to use.</param>
        /// <param name="logger">The logger to use for logging.</param>
        public RebusMessageSerializer(JsonSerializerOptions options, ILogger<RebusMessageSerializer> logger)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger;
        }

        /// <summary>
        /// Serializes the specified message to a transport message.
        /// </summary>
        /// <param name="message">The message to serialize.</param>
        /// <returns>The serialized transport message.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the message is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the message is not of type MessageWrapper.</exception>
        public Task<TransportMessage> Serialize(Message message)
        {
            if (message == null)
            {
                _logger.LogError("Serialize: message is null");
                throw new ArgumentNullException(nameof(message));
            }

            if (message.Body is not MessageWrapper wrapper)
            {
                _logger.LogError("Serialize: message body is not of type MessageWrapper");
                throw new InvalidOperationException("The message body must be of type MessageWrapper.");
            }

            // Garante que o tipo seja armazenado no MessageWrapper
            wrapper.Type = wrapper.Data is TransactionRequestMessageData ? MessageType.TransactionRequestMessageData : MessageType.TransactionResponseMessageData;

            var json = JsonSerializer.Serialize(wrapper, _options);
            var bodyBytes = Encoding.UTF8.GetBytes(json);

            _logger.LogInformation("Serialize: message serialized successfully");

            return Task.FromResult(new TransportMessage(message.Headers, bodyBytes));
        }

        /// <summary>
        /// Deserializes the specified transport message to a message.
        /// </summary>
        /// <param name="transportMessage">The transport message to deserialize.</param>
        /// <returns>The deserialized message.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the transport message is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the message cannot be deserialized to a MessageWrapper.</exception>
        public Task<Message> Deserialize(TransportMessage transportMessage)
        {
            if (transportMessage == null)
            {
                _logger.LogError("Deserialize: transportMessage is null");
                throw new ArgumentNullException(nameof(transportMessage));
            }

            var json = Encoding.UTF8.GetString(transportMessage.Body);
            var wrapper = JsonSerializer.Deserialize<MessageWrapper>(json, _options);

            if (wrapper == null)
            {
                _logger.LogError("Deserialize: failed to deserialize the message");
                throw new InvalidOperationException("Failed to deserialize the message.");
            }

            _logger.LogInformation("Deserialize: message deserialized successfully");

            return Task.FromResult(new Message(transportMessage.Headers, wrapper));
        }
    }
}
