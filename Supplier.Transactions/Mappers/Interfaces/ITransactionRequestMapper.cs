using Supplier.Transactions.Dto.Requests;
using Supplier.Transactions.Messaging.Contracts;
using Supplier.Transactions.Models;

namespace Supplier.Transactions.Mappers.Interfaces
{
    public interface ITransactionRequestMapper
    {
        TransactionMessageData MapToTransactionMessageData(TransactionRequest transactionRequest);
        TransactionRequest MapToTransactionRequest(TransactionRequestDto request);
    }
}
