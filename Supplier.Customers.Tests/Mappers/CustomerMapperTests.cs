using Supplier.Customers.Dto.Requests;
using Supplier.Customers.Mappers;
using Supplier.Customers.Models;

namespace Supplier.Customers.Tests.Mappers
{
    public class CustomerMapperTests
    {
        private readonly CustomerMapper _mapper;

        public CustomerMapperTests()
        {
            _mapper = new CustomerMapper();
        }

        [Fact]
        public void MapToCustomer_ShouldMapCorrectly()
        {
            // Arrange
            var dto = new CustomerRequestDto
            {
                Name = "John Doe",
                Cpf = "12345678901",
                CreditLimit = 1000
            };

            // Act
            var customer = _mapper.MapToCustomer(dto);

            // Assert
            Assert.NotNull(customer);
            Assert.Equal(dto.Name, customer.Name);
            Assert.Equal(dto.Cpf, customer.Cpf);
            Assert.Equal(dto.CreditLimit, customer.CreditLimit);
        }

        [Fact]
        public void MapToCustomerResponseDto_ShouldMapCorrectly()
        {
            // Arrange
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                Name = "John Doe",
                Cpf = "12345678901",
                CreditLimit = 1000
            };

            // Act
            var dto = _mapper.MapToCustomerResponseDto(customer);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(customer.Id, dto.Id);
            Assert.Equal(customer.Name, dto.Name);
            Assert.Equal(customer.Cpf, dto.Cpf);
            Assert.Equal(customer.CreditLimit, dto.CreditLimit);
        }

        [Fact]
        public void MapToCustomerRequestDto_ShouldMapCorrectly()
        {
            // Arrange
            var name = "John Doe";
            var cpf = "12345678901";
            var creditLimit = 1000m;

            // Act
            var dto = _mapper.MapToCustomerRequestDto(name, cpf, creditLimit);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(name, dto.Name);
            Assert.Equal(cpf, dto.Cpf);
            Assert.Equal(creditLimit, dto.CreditLimit);
        }
    }
}
