using Supplier.Transactions.Dto.Requests;
using Supplier.Transactions.Mappers.Interfaces;
using Supplier.Transactions.Messaging.Contracts;
using Supplier.Transactions.Models;

namespace Supplier.Transactions.Mappers
{
    public class TransactionRequestMapper : ITransactionRequestMapper
    {
        public TransactionMessageData MapToTransactionMessageData(TransactionRequest transactionRequest)
        {
            return new TransactionMessageData
            {
                Amount = transactionRequest.Amount,
                CustomerId = transactionRequest.CustomerId,
                TransactionId = transactionRequest.TransactionId,
            };
        }

        public TransactionRequest MapToTransactionRequest(TransactionRequestDto request)
        {
            return new TransactionRequest
            {
                CustomerId = Guid.Parse(request.CustomerId),
                Amount = request.Amount,
                RequestedBy = request.UserId ?? string.Empty,
            };
        }
    }
}
