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
    /// <summary>
    /// Service for managing customers.
    /// </summary>
    public class CustomerService : ICustomerService
    {
        private readonly IValidator<CustomerRequestDto> _customerValidator;
        private readonly ICustomerRepository _customerRepository;
        private readonly ICustomerMapper _customerMapper;
        private readonly IMemoryCache _customerCache;
        private readonly ILogger<CustomerService> _logger;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerService"/> class.
        /// </summary>
        /// <param name="customerValidator">The customer validator.</param>
        /// <param name="repository">The customer repository.</param>
        /// <param name="mapper">The customer mapper.</param>
        /// <param name="cache">The memory cache.</param>
        /// <param name="logger">The logger.</param>
        public CustomerService(
            IValidator<CustomerRequestDto> customerValidator,
            ICustomerRepository repository,
            ICustomerMapper mapper,
            IMemoryCache cache,
            ILogger<CustomerService> logger)
        {
            _customerValidator = customerValidator;
            _customerRepository = repository;
            _customerMapper = mapper;
            _customerCache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new customer asynchronously.
        /// </summary>
        /// <param name="dto">The customer request DTO.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the single customer response DTO.</returns>
        /// <exception cref="ValidationException">Thrown when the customer validation fails.</exception>
        public async Task<SingleCustomerResponseDto> CreateCustomerAsync(CustomerRequestDto dto)
        {
            _logger.LogInformation("Starting customer creation process.");

            var validationResult = await _customerValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Customer validation failed: {Errors}", validationResult.Errors);
                throw new ValidationException(validationResult.Errors);
            }

            var customer = _customerMapper.MapToCustomer(dto);
            await _customerRepository.AddAsync(customer);
            _customerCache.Remove("customers"); // Invalidate cache when a new customer is added

            _logger.LogInformation("Customer created successfully with ID: {CustomerId}", customer.Id);
            return new SingleCustomerResponseDto(customer);
        }

        /// <summary>
        /// Gets customers asynchronously with optional filters.
        /// </summary>
        /// <param name="name">The customer name filter.</param>
        /// <param name="cpf">The customer CPF filter.</param>
        /// <param name="creditLimit">The customer credit limit filter.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the multiple customers response DTO.</returns>
        public async Task<MultipleCustomersResponseDto> GetCustomersAsync(string? name = "", string? cpf = "", decimal? creditLimit = 0)
        {
            _logger.LogInformation("Fetching customers with filters - Name: {Name}, CPF: {Cpf}, CreditLimit: {CreditLimit}", name, cpf, creditLimit);

            var customers = await _customerCache.GetOrCreateAsync("customers", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _cacheDuration;
                var allCustomers = await _customerRepository.GetAllAsync();
                return allCustomers ?? Array.Empty<Customer>();
            }) ?? Array.Empty<Customer>();

            var customerRequestDto = _customerMapper.MapToCustomerRequestDto(name, cpf, creditLimit);
            var filteredCustomers = CustomerFilter.ApplyFilters(customers, customerRequestDto);

            var multipleCustomers = filteredCustomers
                .Select(c => _customerMapper.MapToCustomerResponseDto(c))
                .ToList();

            _logger.LogInformation("Fetched {CustomerCount} customers after applying filters.", multipleCustomers.Count);
            return new MultipleCustomersResponseDto(multipleCustomers);
        }

        /// <summary>
        /// Validates a customer asynchronously based on their ID and a specified amount.
        /// </summary>
        /// <param name="customerId">The ID of the customer to validate.</param>
        /// <param name="amount">The amount to validate against the customer's credit limit.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the customer validation response DTO.</returns>
        public async Task<CustomerValidationResponseDto> ValidateCustomerAsync(Guid customerId, decimal amount)
        {
            var customers = await _customerRepository.GetAllAsync() ?? [];
            var customer = customers.FirstOrDefault(c => c.Id == customerId);
            if (customer == null)
            {
                return new CustomerValidationResponseDto
                {
                    IsValid = false,
                    Message = "Customer not found."
                };
            }

            if (customer.CreditLimit < amount)
            {
                return new CustomerValidationResponseDto
                {
                    IsValid = false,
                    Message = "Insufficient credit limit."
                };
            }

            return new CustomerValidationResponseDto
            {
                IsValid = true,
                Message = "Customer successfully validated."
            };
        }
    }
}
