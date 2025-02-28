using Dapper;
using Supplier.Transactions.Enums;
using Supplier.Transactions.Models;
using Supplier.Transactions.Repositories.Interfaces;
using System.Data;

namespace Supplier.Transactions.Repositories
{
    public class TransactionRequestRepository : ITransactionRequestRepository
    {
        private readonly IDbConnection _db;

        public TransactionRequestRepository(IDbConnection db)
        {
            _db = db;
        }
        public async Task<TransactionRequest?> RegisterTransactionRequestAsync(TransactionRequest transaction)
        {
            const string insertQuery = @"
                INSERT INTO CustomerTransactions (Id, CustomerId, Amount, Status, RequestedBy, RequestedAt, CustomerBlocked) 
                VALUES (@Id, @CustomerId, @Amount, @Status, @RequestedBy, @RequestedAt, @CustomerBlocked);
                SELECT * FROM CustomerTransactions WHERE Id = @Id;";

            var insertedTransaction = await _db.QueryFirstOrDefaultAsync<TransactionRequest>(insertQuery, new
            {
                transaction.Id,
                transaction.CustomerId,
                transaction.Amount,
                Status = TransactionStatus.Pending,
                transaction.RequestedBy,
                RequestedAt = DateTime.UtcNow,
                CustomerBlocked = transaction.CustomerBlocked ? 1 : 0
            });

            return insertedTransaction;
        }

        public async Task UpdateTransactionRequestAsync(TransactionRequest transactionRequest)
        {
            const string updateQuery = @"
                UPDATE CustomerTransactions 
                SET Amount = @Amount, 
                    Status = @Status, 
                    UpdatedBy = @UpdatedBy, 
                    UpdatedAt = @UpdatedAt, 
                    Detail = @Detail 
                WHERE Id = @Id;";

            await _db.ExecuteAsync(updateQuery, new
            {
                transactionRequest.Id,
                transactionRequest.Amount,
                transactionRequest.Status,
                transactionRequest.UpdatedBy,
                UpdatedAt = DateTime.UtcNow,
                transactionRequest.Detail
            });
        }

        public async Task UpdateTransactionRequestAsync(Guid transactionId)
        {
            const string updateStatusQuery = @"
                UPDATE CustomerTransactions 
                SET Status = @Status, 
                    UpdatedBy = @UpdatedBy, 
                    UpdatedAt = @UpdatedAt 
                WHERE TransactionId = @TransactionId;";

            await _db.ExecuteAsync(updateStatusQuery, new
            {
                TransactionId = transactionId,
                Status = TransactionStatus.Completed,
                UpdatedBy = "System",
                UpdatedAt = DateTime.UtcNow
            });
        }

        public async Task UpdateTransactionRequestAsync(Guid transactionId, string message)
        {
            const string updateFailureQuery = @"
                UPDATE CustomerTransactions 
                SET Status = @Status, 
                    UpdatedBy = @UpdatedBy, 
                    UpdatedAt = @UpdatedAt, 
                    Detail = @Detail 
                WHERE TransactionId = @TransactionId;";

            await _db.ExecuteAsync(updateFailureQuery, new
            {
                TransactionId = transactionId,
                Status = TransactionStatus.Failed,
                UpdatedBy = "System",
                UpdatedAt = DateTime.UtcNow,
                Detail = message
            });
        }
    }
}