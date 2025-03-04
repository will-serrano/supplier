using Supplier.Contracts.Transactions.Requests;
using Supplier.Transactions.Dto.Requests;
using Supplier.Transactions.Mappers.Interfaces;
using Supplier.Transactions.Models;

namespace Supplier.Transactions.Mappers
{
    /// <summary>
    /// Maps transaction request data between different representations.
    /// </summary>
    public class TransactionRequestMapper : ITransactionRequestMapper
    {
        /// <summary>
        /// Maps a <see cref="TransactionRequest"/> to a <see cref="TransactionRequestMessageData"/>.
        /// </summary>
        /// <param name="transactionRequest">The transaction request to map.</param>
        /// <returns>A <see cref="TransactionRequestMessageData"/> representation of the transaction request.</returns>
        public TransactionRequestMessageData MapToTransactionMessageData(TransactionRequest transactionRequest)
        {
            return new TransactionRequestMessageData
            {
                Amount = transactionRequest.Amount,
                CustomerId = transactionRequest.CustomerId,
                TransactionId = transactionRequest.TransactionId,
            };
        }

        /// <summary>
        /// Maps a <see cref="TransactionRequestDto"/> to a <see cref="TransactionRequest"/>.
        /// </summary>
        /// <param name="request">The transaction request DTO to map.</param>
        /// <returns>A <see cref="TransactionRequest"/> representation of the transaction request DTO.</returns>
        public TransactionRequest MapToTransactionRequest(TransactionRequestDto request)
        {
            return new TransactionRequest
            {
                CustomerId = string.IsNullOrEmpty(request.CustomerId) ? Guid.Empty : Guid.Parse(request.CustomerId),
                Amount = request.Amount.GetValueOrDefault(),
            };
        }
    }
}
