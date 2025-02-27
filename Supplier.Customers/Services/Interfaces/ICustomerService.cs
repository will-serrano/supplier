using Supplier.Customers.Dto.Requests;
using Supplier.Customers.Dto.Responses;
using Supplier.Customers.Models;

namespace Supplier.Customers.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<CustomerResponseDto> RegisterCustomerAsync(CustomerRequestDto dto);
        Task<IEnumerable<Customer>> GetCustomersAsync();
    }
}
