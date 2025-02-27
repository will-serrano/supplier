using Supplier.Customers.Dto.Requests;
using Supplier.Customers.Dto.Responses;
using Supplier.Customers.Mappers.Interfaces;
using Supplier.Customers.Models;

namespace Supplier.Customers.Mappers
{
    public class CustomerMapper : ICustomerMapper
    {
        public Customer MapToCustomer(CustomerRequestDto dto)
        {
            return new Customer
            {
                Name = dto.Name,
                Cpf = dto.Cpf,
                CreditLimit = dto.CreditLimit
            };
        }

        public CustomerResponseDto MapToCustomerResponseDto(Customer customer)
        {
            return new CustomerResponseDto
            {
                Id = customer.Id,
                Name = customer.Name,
                Cpf = customer.Cpf,
                CreditLimit = customer.CreditLimit
            };
        }

        public CustomerRequestDto MapToCustomerRequestDto(string? name, string? cpf, decimal? creditLimit)
        {
            return new CustomerRequestDto
            {
                Name = name ?? string.Empty,
                Cpf = cpf ?? string.Empty,
                CreditLimit = creditLimit ?? 0
            };
        }
    }
}
