using Rebus.Messages;
using Rebus.Serialization;
using Supplier.Contracts.Transactions;
using System.Text;
using System.Text.Json;

namespace Supplier.Customers.Messaging
{
    public class RebusMessageSerializer : ISerializer
    {
        private readonly JsonSerializerOptions _jsonOptions;

        public RebusMessageSerializer()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                // Caso precise de configuração adicional, como converters, adicione aqui.
            };
        }

        public Task<TransportMessage> Serialize(Message message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            // Assume que o corpo é um MessageWrapper
            string json = JsonSerializer.Serialize(message.Body, _jsonOptions);
            byte[] bodyBytes = Encoding.UTF8.GetBytes(json);

            var transportMessage = new TransportMessage(message.Headers, bodyBytes);
            return Task.FromResult(transportMessage);
        }

        public Task<Message> Deserialize(TransportMessage transportMessage)
        {
            if (transportMessage == null)
                throw new ArgumentNullException(nameof(transportMessage));

            string json = Encoding.UTF8.GetString(transportMessage.Body);

            // Desserializa para MessageWrapper
            var messageWrapper = JsonSerializer.Deserialize<MessageWrapper>(json, _jsonOptions);
            if (messageWrapper == null)
            {
                throw new InvalidOperationException("Não foi possível desserializar a mensagem para MessageWrapper.");
            }

            // Cria a mensagem do Rebus com os headers originais e o objeto desserializado
            var message = new Message(transportMessage.Headers, messageWrapper);
            return Task.FromResult(message);
        }
    }
}
