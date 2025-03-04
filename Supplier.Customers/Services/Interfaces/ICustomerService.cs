using Supplier.Customers.Dto.Requests;
using Supplier.Customers.Dto.Responses;
using Supplier.Customers.Models;

namespace Supplier.Customers.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<SingleCustomerResponseDto> CreateCustomerAsync(CustomerRequestDto dto);
        Task<MultipleCustomersResponseDto> GetCustomersAsync(string? name, string? cpf, decimal? creditLimit);
        Task<CustomerValidationResponseDto> ValidateCustomerAsync(Guid customerId, decimal amount);
    }
}
