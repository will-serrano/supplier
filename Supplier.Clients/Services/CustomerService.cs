using Microsoft.Extensions.Caching.Memory;
using Supplier.Customers.Dto.Requests;
using Supplier.Customers.Dto.Responses;
using Supplier.Customers.Models;
using Supplier.Customers.Repositories.Interfaces;
using Supplier.Customers.Services.Interfaces;

namespace Supplier.Customers.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _repository;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10);

        public CustomerService(ICustomerRepository repository, IMemoryCache cache)
        {
            _repository = repository;
            _cache = cache;
        }

        public async Task<CustomerResponseDto> RegisterCustomerAsync(CustomerRequestDto dto)
        {
            if (await _repository.ExistsAsync(dto.CPF))
                throw new Exception("Cliente já cadastrado.");

            if (dto.CreditLimit < 0)
                throw new Exception("O limite de crédito não pode ser negativo.");

            var customer = new Customer { Name = dto.Name, CPF = dto.CPF, CreditLimit = dto.CreditLimit };
            await _repository.AddAsync(customer);

            _cache.Remove("customers"); // Invalida cache ao cadastrar novo cliente

            return new CustomerResponseDto { IdCliente = customer.Id, Status = "OK" };
        }

        public async Task<IEnumerable<Customer>> GetCustomersAsync()
        {
            return await _cache.GetOrCreateAsync("customers", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _cacheDuration;
                return await _repository.GetAllAsync();
            });
        }
    }
}
