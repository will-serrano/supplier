using Rebus.Messages;
using Rebus.Serialization;
using System.Text.Json;
using System.Text;
using Supplier.Contracts.Transactions;

namespace Supplier.Transactions.Messaging
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
            ArgumentNullException.ThrowIfNull(message);

            // Assume que o corpo é um MessageWrapper
            string json = JsonSerializer.Serialize(message.Body, _jsonOptions);
            byte[] bodyBytes = Encoding.UTF8.GetBytes(json);

            var transportMessage = new TransportMessage(message.Headers, bodyBytes);
            return Task.FromResult(transportMessage);
        }

        public Task<Message> Deserialize(TransportMessage transportMessage)
        {
            ArgumentNullException.ThrowIfNull(transportMessage);

            string json = Encoding.UTF8.GetString(transportMessage.Body);

            // Desserializa para MessageWrapper
            var messageWrapper = JsonSerializer.Deserialize<MessageWrapper>(json, _jsonOptions)
                ?? throw new InvalidOperationException("Não foi possível desserializar a mensagem para MessageWrapper.");

            // Cria a mensagem do Rebus com os headers originais e o objeto desserializado
            var message = new Message(transportMessage.Headers, messageWrapper);
            return Task.FromResult(message);
        }
    }
}
