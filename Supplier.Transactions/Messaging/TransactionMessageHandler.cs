using Rebus.Handlers;
using Supplier.Contracts.Transactions;
using Supplier.Contracts.Transactions.Responses;
using Supplier.Transactions.Enums;
using Supplier.Transactions.Models;
using Supplier.Transactions.Repositories.Interfaces;

namespace Supplier.Transactions.Messaging
{
    /// <summary>
    /// Handles transaction messages.
    /// </summary>
    public class TransactionMessageHandler : IHandleMessages<MessageWrapper>
    {
        private readonly ITransactionRequestRepository _transactionRequestRepository;
        private readonly ILogger<TransactionMessageHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionMessageHandler"/> class.
        /// </summary>
        /// <param name="transactionRequestRepository">The transaction request repository.</param>
        /// <param name="logger">The logger.</param>
        public TransactionMessageHandler(ITransactionRequestRepository transactionRequestRepository, ILogger<TransactionMessageHandler> logger)
        {
            _transactionRequestRepository = transactionRequestRepository;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Handles the specified message.
        /// </summary>
        /// <param name="message">The message to handle.</param>
        public async Task Handle(MessageWrapper message)
        {
            _logger.LogInformation("Message received: {@Message}", message);

            if (message.Data == null)
            {
                _logger.LogError("Received message with empty data.");
                return;
            }

            if (message.Data is not TransactionResponseMessageData transactionData)
            {
                _logger.LogError("Failed to convert message to TransactionResponseMessageData.");
                return;
            }

            if (!transactionData!.IsSuccess)
            {
                _logger.LogError("Transaction rejected: {TransactionId} - Message: {Message}", transactionData.TransactionId, transactionData.Message);

                await _transactionRequestRepository.UpdateTransactionRequestAsync(transactionData.TransactionId, transactionData.Message);
                return;
            }

            _logger.LogInformation("Transaction completed: {TransactionId} - New Limit: {NewLimit}", transactionData.TransactionId, transactionData.NewLimit);
            await _transactionRequestRepository.UpdateTransactionRequestAsync(transactionData.TransactionId);
        }
    }
}
