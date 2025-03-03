using Supplier.Transactions.Dto.Requests;
using Supplier.Transactions.Dto.Responses;

namespace Supplier.Transactions.Services.Interfaces
{
    /// <summary>
    /// Service for handling transaction requests.
    /// </summary>
    public interface ITransactionRequestService
    {
        /// <summary>
        /// Requests a transaction asynchronously.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<TransactionResponseDto> RequestTransactionAsync(TransactionRequestDto request, Guid userId, string token);
        /// <summary>
        /// Validates a transaction request.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<(bool IsValid, string ErrorMessage, Guid UserId)> ValidateRequest(TransactionRequestDto request, string token);
    }
}
