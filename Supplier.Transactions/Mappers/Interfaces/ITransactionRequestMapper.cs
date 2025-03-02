using Supplier.Contracts.Transactions.Requests;
using Supplier.Transactions.Dto.Requests;
using Supplier.Transactions.Models;

namespace Supplier.Transactions.Mappers.Interfaces
{
    public interface ITransactionRequestMapper
    {
        TransactionRequestMessageData MapToTransactionMessageData(TransactionRequest transactionRequest);
        TransactionRequest MapToTransactionRequest(TransactionRequestDto request);
    }
}
