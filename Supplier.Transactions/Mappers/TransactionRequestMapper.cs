using Supplier.Contracts.Transactions.Requests;
using Supplier.Transactions.Dto.Requests;
using Supplier.Transactions.Mappers.Interfaces;
using Supplier.Transactions.Models;

namespace Supplier.Transactions.Mappers
{
    public class TransactionRequestMapper : ITransactionRequestMapper
    {
        public TransactionRequestMessageData MapToTransactionMessageData(TransactionRequest transactionRequest)
        {
            return new TransactionRequestMessageData
            {
                Amount = transactionRequest.Amount,
                CustomerId = transactionRequest.CustomerId,
                TransactionId = transactionRequest.TransactionId
            };
        }

        public TransactionRequest MapToTransactionRequest(TransactionRequestDto request)
        {
            return new TransactionRequest
            {
                CustomerId = request.CustomerId != null ? Guid.Parse(request.CustomerId) : Guid.Empty,
                Amount = request.Amount ?? 0,
                RequestedBy = request.UserId ?? string.Empty,
            };
        }
    }
}
