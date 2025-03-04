using Supplier.Transactions.Enums;
using Supplier.Transactions.Models;

namespace Supplier.Transactions.Repositories.Interfaces
{
    public interface ITransactionRequestRepository
    {
        Task<bool> IsCustomerBlockedAsync(string customerId);
        Task<TransactionRequest?> RegisterTransactionRequestAsync(TransactionRequest transaction);
        Task UpdateTransactionRequestAsync(TransactionRequest transactionRequest);
        Task UpdateTransactionRequestAsync(Guid transactionId);
        Task UpdateTransactionRequestAsync(Guid transactionId, string message);
    }
}
