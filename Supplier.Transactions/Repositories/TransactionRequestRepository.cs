using Dapper;
using Supplier.Transactions.Configuration.Interfaces;
using Supplier.Transactions.Enums;
using Supplier.Transactions.Models;
using Supplier.Transactions.Repositories.Interfaces;
using System.Data;

namespace Supplier.Transactions.Repositories
{
    /// <summary>
    /// Repository for handling transaction requests.
    /// </summary>
    public class TransactionRequestRepository : ITransactionRequestRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        private readonly IDapperWrapper _dapperWrapper;
        private readonly ILogger<TransactionRequestRepository> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionRequestRepository"/> class.
        /// </summary>
        /// <param name="dbConnectionFactory">The database connection factory.</param>
        /// <param name="dapperWrapper">The Dapper wrapper instance.</param>
        /// <param name="logger">The logger instance.</param>
        public TransactionRequestRepository(IDbConnectionFactory dbConnectionFactory, IDapperWrapper dapperWrapper, ILogger<TransactionRequestRepository> logger)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _dapperWrapper = dapperWrapper;
            _logger = logger;
        }

        /// <summary>
        /// Registers a new transaction request asynchronously.
        /// </summary>
        /// <param name="transaction">The transaction request to register.</param>
        /// <returns>The registered transaction request, or null if registration failed.</returns>
        public async Task<TransactionRequest?> RegisterTransactionRequestAsync(TransactionRequest transaction)
        {
            _logger.LogInformation("Registering transaction request: {TransactionId}", transaction.Id);

            const string insertQuery = @"
                            INSERT INTO CustomerTransactions (Id, CustomerId, Amount, Status, RequestedBy, RequestedAt, CustomerBlocked) 
                            VALUES (@Id, @CustomerId, @Amount, @Status, @RequestedBy, @RequestedAt, @CustomerBlocked);
                            SELECT * FROM CustomerTransactions WHERE Id = @Id;";

            using var connection = _dbConnectionFactory.CreateConnection();
            var insertedTransaction = await _dapperWrapper.QueryFirstOrDefaultAsync<TransactionRequest>(connection, new CommandDefinition(insertQuery, new
            {
                transaction.Id,
                transaction.CustomerId,
                transaction.Amount,
                Status = TransactionStatus.Pending,
                transaction.RequestedBy,
                RequestedAt = DateTime.UtcNow,
                CustomerBlocked = transaction.CustomerBlocked ? 1 : 0
            }));

            if (insertedTransaction != null)
            {
                _logger.LogInformation("Transaction request registered successfully: {TransactionId}", transaction.Id);
            }
            else
            {
                _logger.LogWarning("Failed to register transaction request: {TransactionId}", transaction.Id);
            }

            return insertedTransaction;
        }

        /// <summary>
        /// Updates an existing transaction request asynchronously.
        /// </summary>
        /// <param name="transactionRequest">The transaction request to update.</param>
        public async Task UpdateTransactionRequestAsync(TransactionRequest transactionRequest)
        {
            _logger.LogInformation("Updating transaction request: {TransactionId}", transactionRequest.Id);

            const string updateQuery = @"
                            UPDATE CustomerTransactions 
                            SET Amount = @Amount, 
                                Status = @Status, 
                                UpdatedBy = @UpdatedBy, 
                                UpdatedAt = @UpdatedAt, 
                                Detail = @Detail 
                            WHERE Id = @Id;";

            using var connection = _dbConnectionFactory.CreateConnection();
            await _dapperWrapper.ExecuteAsync(connection, new CommandDefinition(updateQuery, new
            {
                transactionRequest.Id,
                transactionRequest.Amount,
                transactionRequest.Status,
                transactionRequest.UpdatedBy,
                UpdatedAt = DateTime.UtcNow,
                transactionRequest.Detail
            }));

            _logger.LogInformation("Transaction request updated successfully: {TransactionId}", transactionRequest.Id);
        }

        /// <summary>
        /// Updates the status of a transaction request to Completed asynchronously.
        /// </summary>
        /// <param name="transactionId">The ID of the transaction to update.</param>
        public async Task UpdateTransactionRequestAsync(Guid transactionId)
        {
            _logger.LogInformation("Updating transaction request status to Completed: {TransactionId}", transactionId);

            const string updateStatusQuery = @"
                            UPDATE CustomerTransactions 
                            SET Status = @Status, 
                                UpdatedBy = @UpdatedBy, 
                                UpdatedAt = @UpdatedAt 
                            WHERE Id = @TransactionId;";

            using var connection = _dbConnectionFactory.CreateConnection();
            await _dapperWrapper.ExecuteAsync(connection, new CommandDefinition(updateStatusQuery, new
            {
                TransactionId = transactionId,
                Status = TransactionStatus.Completed,
                UpdatedBy = "System",
                UpdatedAt = DateTime.UtcNow
            }));

            _logger.LogInformation("Transaction request status updated to Completed: {TransactionId}", transactionId);
        }

        /// <summary>
        /// Updates the status of a transaction request to Failed asynchronously.
        /// </summary>
        /// <param name="transactionId">The ID of the transaction to update.</param>
        /// <param name="message">The failure message.</param>
        public async Task UpdateTransactionRequestAsync(Guid transactionId, string message)
        {
            _logger.LogInformation("Updating transaction request status to Failed: {TransactionId}, Message: {Message}", transactionId, message);

            const string updateFailureQuery = @"
                            UPDATE CustomerTransactions 
                            SET Status = @Status, 
                                UpdatedBy = @UpdatedBy, 
                                UpdatedAt = @UpdatedAt, 
                                Detail = @Detail 
                            WHERE Id = @TransactionId;";

            using var connection = _dbConnectionFactory.CreateConnection();
            await _dapperWrapper.ExecuteAsync(connection, new CommandDefinition(updateFailureQuery, new
            {
                TransactionId = transactionId,
                Status = TransactionStatus.Failed,
                UpdatedBy = "System",
                UpdatedAt = DateTime.UtcNow,
                Detail = message
            }));

            _logger.LogInformation("Transaction request status updated to Failed: {TransactionId}", transactionId);
        }
    }
}