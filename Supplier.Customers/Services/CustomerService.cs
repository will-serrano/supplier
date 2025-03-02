using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using Supplier.Customers.Dto.Requests;
using Supplier.Customers.Dto.Responses;
using Supplier.Customers.Filters;
using Supplier.Customers.Mappers.Interfaces;
using Supplier.Customers.Models;
using Supplier.Customers.Repositories.Interfaces;
using Supplier.Customers.Services.Interfaces;

namespace Supplier.Customers.Services
{
    public class CustomerService(
        IValidator<CustomerRequestDto> customerValidator, 
        ICustomerRepository repository, 
        ICustomerMapper mapper, 
        IMemoryCache cache) : ICustomerService
    {
        private readonly IValidator<CustomerRequestDto> _customerValidator = customerValidator;
        private readonly ICustomerRepository _customerRepository = repository;
        private readonly ICustomerMapper _customerMapper = mapper;
        private readonly IMemoryCache _customerCache = cache;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10);

        public async Task<SingleCustomerResponseDto> CreateCustomerAsync(CustomerRequestDto dto)
        {
            var validationResult = await _customerValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var customer = _customerMapper.MapToCustomer(dto);

            await _customerRepository.AddAsync(customer);

            _customerCache.Remove("customers"); // Invalida cache ao cadastrar novo cliente

            return new SingleCustomerResponseDto(customer);
        }

        public async Task<MultipleCustomersResponseDto> GetCustomersAsync(string? name, string? cpf, decimal? creditLimit)
        {
            var customers =  await _customerCache.GetOrCreateAsync("customers", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _cacheDuration;
                var customers = await _customerRepository.GetAllAsync();
                return customers ?? [];
            }) ?? [];

            var customerRequestDto = _customerMapper.MapToCustomerRequestDto(name, cpf, creditLimit);

            var filteredCustomers = CustomerFilter.ApplyFilters(customers, customerRequestDto);

            var multipleCustomers = filteredCustomers
                .Select(c => _customerMapper.MapToCustomerResponseDto(c))
                .ToList();

            return new MultipleCustomersResponseDto(multipleCustomers);
        }
    }
}
