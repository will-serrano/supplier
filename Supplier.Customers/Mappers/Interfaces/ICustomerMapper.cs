using Supplier.Customers.Dto.Requests;
using Supplier.Customers.Dto.Responses;
using Supplier.Customers.Models;

namespace Supplier.Customers.Mappers.Interfaces
{
    public interface ICustomerMapper
    {
        Customer MapToCustomer(CustomerRequestDto dto);
        CustomerResponseDto MapToCustomerResponseDto(Customer customer);
        CustomerRequestDto MapToCustomerRequestDto(string? name, string? cpf, decimal? creditLimit);
    }
}
