using Rebus.Handlers;
using Supplier.Contracts.Transactions;
using Supplier.Contracts.Transactions.Responses;
using Supplier.Transactions.Repositories.Interfaces;

namespace Supplier.Transactions.Messaging
{
    public class TransactionMessageHandler : IHandleMessages<MessageWrapper>
    {
        private readonly ITransactionRequestRepository _transactionRequestRepository;
        private readonly ILogger<TransactionMessageHandler> _logger;

        public TransactionMessageHandler(ITransactionRequestRepository transactionRequestRepository, ILogger<TransactionMessageHandler> logger)
        {
            _transactionRequestRepository = transactionRequestRepository;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(MessageWrapper message)
        {
            _logger.LogInformation("Mensagem recebida: {@Message}", message);

            if (message.Data == null)
            {
                _logger.LogError("Mensagem recebida com dados vazios.");
                return;
            }

            if (message.Data is not TransactionResponseMessageData transactionData)
            {
                _logger.LogError("Falha ao converter a mensagem para TransactionResponseMessageData.");
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
    }
}
