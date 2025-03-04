using Supplier.Transactions.HttpClients.Dto;
using Supplier.Transactions.Models;

namespace Supplier.Transactions.HttpClients.Interfaces
{
    public interface ICustomerValidationClient
    {
        Task<CustomerValidationResultDto> ValidateCustomerAsync(TransactionRequest transactionRequest, string token);
    }
}
