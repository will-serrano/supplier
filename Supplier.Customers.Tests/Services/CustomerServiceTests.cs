using FluentValidation;
using FluentAssertions;
using FluentValidation.Results;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Supplier.Customers.Dto.Requests;
using Supplier.Customers.Dto.Responses;
using Supplier.Customers.Mappers.Interfaces;
using Supplier.Customers.Models;
using Supplier.Customers.Repositories.Interfaces;
using Supplier.Customers.Services;
using System.ComponentModel.DataAnnotations;

namespace Supplier.Customers.Tests.Services
{
    public class CustomerServiceTests
    {
        private readonly Mock<IValidator<CustomerRequestDto>> _mockValidator;
        private readonly Mock<ICustomerRepository> _mockRepository;
        private readonly Mock<ICustomerMapper> _mockMapper;
        private readonly Mock<IMemoryCache> _mockCache;
        private readonly Mock<ILogger<CustomerService>> _mockLogger;
        private readonly CustomerService _customerService;

        public CustomerServiceTests()
        {
            _mockValidator = new Mock<IValidator<CustomerRequestDto>>();
            _mockRepository = new Mock<ICustomerRepository>();
            _mockMapper = new Mock<ICustomerMapper>();
            _mockCache = new Mock<IMemoryCache>();
            _mockLogger = new Mock<ILogger<CustomerService>>();

            _customerService = new CustomerService(
                _mockValidator.Object,
                _mockRepository.Object,
                _mockMapper.Object,
                _mockCache.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task CreateCustomerAsync_ShouldCreateCustomer_WhenValidationPasses()
        {
            // Arrange
            var customerRequestDto = new CustomerRequestDto { Name = "John Doe", Cpf = "12345678901", CreditLimit = 1000 };
            var customer = new Customer { Id = Guid.NewGuid(), Name = "John Doe", Cpf = "12345678901", CreditLimit = 1000 };
            var validationResult = new FluentValidation.Results.ValidationResult();

            _mockValidator.Setup(v => v.ValidateAsync(customerRequestDto, default)).ReturnsAsync(validationResult);
            _mockMapper.Setup(m => m.MapToCustomer(customerRequestDto)).Returns(customer);
            _mockRepository.Setup(r => r.AddAsync(customer)).Returns(Task.CompletedTask);
            _mockCache.Setup(c => c.Remove("customers"));

            // Act
            var result = await _customerService.CreateCustomerAsync(customerRequestDto);

            // Assert
            result.Should().NotBeNull();
            result.IdCliente.Should().Be(customer.Id);
            _mockValidator.Verify(v => v.ValidateAsync(customerRequestDto, default), Times.Once);
            _mockMapper.Verify(m => m.MapToCustomer(customerRequestDto), Times.Once);
            _mockRepository.Verify(r => r.AddAsync(customer), Times.Once);
            _mockCache.Verify(c => c.Remove("customers"), Times.Once);
        }

        [Fact]
        public async Task CreateCustomerAsync_ShouldThrowValidationException_WhenValidationFails()
        {
            // Arrange
            var customerRequestDto = new CustomerRequestDto { Name = "John Doe", Cpf = "12345678901", CreditLimit = 1000 };
            var validationResult = new FluentValidation.Results.ValidationResult(new List<ValidationFailure> { new ValidationFailure("Name", "Name is required") });

            _mockValidator.Setup(v => v.ValidateAsync(customerRequestDto, default)).ReturnsAsync(validationResult);

            // Act
            Func<Task> act = async () => await _customerService.CreateCustomerAsync(customerRequestDto);

            // Assert
            await act.Should().ThrowAsync<FluentValidation.ValidationException>();
            _mockValidator.Verify(v => v.ValidateAsync(customerRequestDto, default), Times.Once);
            _mockMapper.Verify(m => m.MapToCustomer(It.IsAny<CustomerRequestDto>()), Times.Never);
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<Customer>()), Times.Never);
            _mockCache.Verify(c => c.Remove("customers"), Times.Never);
        }

        [Fact]
        public async Task GetCustomersAsync_ShouldReturnFilteredCustomers()
        {
            // Arrange
            var customers = new List<Customer>
            {
                new Customer { Id = Guid.NewGuid(), Name = "John Doe", Cpf = "12345678901", CreditLimit = 1000 },
                new Customer { Id = Guid.NewGuid(), Name = "Jane Who", Cpf = "09876543210", CreditLimit = 2000 }
            };
            var customerRequestDto = new CustomerRequestDto { Name = "John Doe", Cpf = "12345678901", CreditLimit = 1000 };
            var customerResponseDtos = customers.Select(c => new CustomerResponseDto { Id = c.Id, Name = c.Name, Cpf = c.Cpf, CreditLimit = c.CreditLimit }).ToList();

            object cacheEntry = customers;
            _mockCache.Setup(c => c.TryGetValue("customers", out cacheEntry)).Returns(true);
            _mockMapper.Setup(m => m.MapToCustomerRequestDto("John Doe", "12345678901", 1000)).Returns(customerRequestDto);
            _mockMapper.Setup(m => m.MapToCustomerResponseDto(It.IsAny<Customer>())).Returns((Customer c) => new CustomerResponseDto { Id = c.Id, Name = c.Name, Cpf = c.Cpf, CreditLimit = c.CreditLimit });

            // Act
            var result = await _customerService.GetCustomersAsync("John Doe", "12345678901", 1000);

            // Assert
            result.Should().NotBeNull();
            result.Customers.Should().BeEquivalentTo(customerResponseDtos.Where(c => c.Cpf == "12345678901"));
            _mockCache.Verify(c => c.TryGetValue("customers", out cacheEntry), Times.Once);
            _mockMapper.Verify(m => m.MapToCustomerRequestDto("John Doe", "12345678901", 1000), Times.Once);
        }

        [Fact]
        public async Task GetAllCustomersAsync_ShouldReturnFilteredCustomers()
        {
            // Arrange
            var customers = new List<Customer>
            {
                new Customer { Id = Guid.NewGuid(), Name = "John Doe", Cpf = "12345678901", CreditLimit = 1000 },
                new Customer { Id = Guid.NewGuid(), Name = "Jane Who", Cpf = "09876543210", CreditLimit = 2000 }
            };
            var customerResponseDtos = customers.Select(c => new CustomerResponseDto { Id = c.Id, Name = c.Name, Cpf = c.Cpf, CreditLimit = c.CreditLimit }).ToList();

            object cacheEntry = customers;
            _mockCache.Setup(c => c.TryGetValue("customers", out cacheEntry)).Returns(true);
            _mockMapper.Setup(m => m.MapToCustomerRequestDto(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>())).Returns(new CustomerRequestDto() { Name = string.Empty, Cpf = string.Empty, CreditLimit = 0 });
            _mockMapper.Setup(m => m.MapToCustomerResponseDto(It.IsAny<Customer>())).Returns((Customer c) => new CustomerResponseDto { Id = c.Id, Name = c.Name, Cpf = c.Cpf, CreditLimit = c.CreditLimit });

            // Act
            var result = await _customerService.GetCustomersAsync();

            // Assert
            result.Should().NotBeNull();
            result.Customers.Should().BeEquivalentTo(customerResponseDtos);
            _mockCache.Verify(c => c.TryGetValue("customers", out cacheEntry), Times.Once);
            _mockMapper.Verify(m => m.MapToCustomerResponseDto(It.IsAny<Customer>()), Times.Exactly(customers.Count));
        }

        [Fact]
        public async Task ValidateCustomerAsync_ShouldReturnInvalid_WhenCustomerNotFound()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync((IEnumerable<Customer>)null);

            // Act
            var result = await _customerService.ValidateCustomerAsync(customerId, 100);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Message.Should().Be("Customer not found.");
        }

        [Fact]
        public async Task ValidateCustomerAsync_ShouldReturnInvalid_WhenCreditLimitIsInsufficient()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var customers = new List<Customer>
            {
                new Customer { Id = customerId, CreditLimit = 50 }
            };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(customers);

            // Act
            var result = await _customerService.ValidateCustomerAsync(customerId, 100);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Message.Should().Be("Insufficient credit limit.");
        }

        [Fact]
        public async Task ValidateCustomerAsync_ShouldReturnValid_WhenCustomerIsValid()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var customers = new List<Customer>
            {
                new Customer { Id = customerId, CreditLimit = 200 }
            };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(customers);

            // Act
            var result = await _customerService.ValidateCustomerAsync(customerId, 100);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Message.Should().Be("Customer successfully validated.");
        }
    }
}
