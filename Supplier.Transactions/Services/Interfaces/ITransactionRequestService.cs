using Supplier.Transactions.Dto.Requests;
using Supplier.Transactions.Dto.Responses;

namespace Supplier.Transactions.Services.Interfaces
{
    public interface ITransactionRequestService
    {
        Task<TransactionResponseDto> RequestTransactionAsync(TransactionRequestDto request, string token);
    }
}
