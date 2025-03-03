using Supplier.Customers.Dto.Requests;
using Supplier.Customers.Dto.Responses;
using Supplier.Customers.Mappers.Interfaces;
using Supplier.Customers.Models;

namespace Supplier.Customers.Mappers
{
    /// <summary>
    /// Provides methods to map between Customer and DTOs.
    /// </summary>
    public class CustomerMapper : ICustomerMapper
    {
        /// <summary>
        /// Maps a CustomerRequestDto to a Customer.
        /// </summary>
        /// <param name="dto">The CustomerRequestDto to map.</param>
        /// <returns>A Customer object.</returns>
        public Customer MapToCustomer(CustomerRequestDto dto)
        {
            return new Customer
            {
                Name = dto.Name ?? string.Empty,
                Cpf = dto.Cpf ?? string.Empty,
                CreditLimit = dto.CreditLimit
            };
        }

        /// <summary>
        /// Maps a Customer to a CustomerResponseDto.
        /// </summary>
        /// <param name="customer">The Customer to map.</param>
        /// <returns>A CustomerResponseDto object.</returns>
        public CustomerResponseDto MapToCustomerResponseDto(Customer customer)
        {
            return new CustomerResponseDto
            {
                Id = customer.Id,
                Name = customer.Name ?? string.Empty,
                Cpf = customer.Cpf ?? string.Empty,
                CreditLimit = customer.CreditLimit
            };
        }

        /// <summary>
        /// Maps individual properties to a CustomerRequestDto.
        /// </summary>
        /// <param name="name">The name of the customer.</param>
        /// <param name="cpf">The CPF of the customer.</param>
        /// <param name="creditLimit">The credit limit of the customer.</param>
        /// <returns>A CustomerRequestDto object.</returns>
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
