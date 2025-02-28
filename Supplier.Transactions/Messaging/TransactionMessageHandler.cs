using Rebus.Bus;
using Rebus.Handlers;
using Supplier.Transactions.Messaging.Contracts;
using Supplier.Transactions.Repositories.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Supplier.Transactions.Messaging
{
    public class TransactionMessageHandler : IHandleMessages<TransactionMessage>
    {
        private readonly ITransactionRequestRepository _transactionRequestRepository;
        private readonly IBus _bus;
        private readonly ILogger<TransactionMessageHandler> _logger;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public TransactionMessageHandler(ITransactionRequestRepository transactionRequestRepository, IBus bus, ILogger<TransactionMessageHandler> logger)
        {
            _transactionRequestRepository = transactionRequestRepository;
            _bus = bus;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(TransactionMessage message)
        {
            _logger.LogInformation("Mensagem recebida: {@Message}", message);

            if (string.IsNullOrWhiteSpace(message.Data))
            {
                _logger.LogError("Mensagem recebida com dados vazios.");
                return;
            }

            if (!TryDeserializeMessage(message.Data, out var transactionData))
            {
                _logger.LogError("Falha ao desserializar a mensagem: {Data}", message.Data);
                return;
            }

            if (!transactionData!.IsSuccess)
            {
                _logger.LogError("Transação rejeitada: {TransactionId} - Mensagem: {Message}", transactionData.TransactionId, transactionData.Message);
                await _transactionRequestRepository.UpdateTransactionRequestAsync(transactionData.TransactionId, transactionData.Message);
                return;
            }


            _logger.LogInformation("Transação finalizada: {TransactionId} - Limite Atualizado: {NewLimit}", transactionData.TransactionId, transactionData.NewLimit);
            await _transactionRequestRepository.UpdateTransactionRequestAsync(transactionData.TransactionId);

        }

        private static bool TryDeserializeMessage(string json, out CustomerUpdateResponseMessageData? data)
        {
            try
            {
                data = JsonSerializer.Deserialize<CustomerUpdateResponseMessageData>(json, _jsonOptions);
                return data != null;
            }
            catch (JsonException)
            {
                data = null;
                return false;
            }
        }
    }
}
