using Dapper;
using Microsoft.Extensions.Logging;
using Moq;
using Supplier.Customers.Configuration.Interfaces;
using Supplier.Customers.Models;
using Supplier.Customers.Repositories;
using Supplier.Customers.Repositories.Interfaces;
using System.Data;

namespace Supplier.Customers.Tests.Repositories
{
    public class CustomerRepositoryTests
    {
        private readonly Mock<IDbConnectionFactory> _dbConnectionFactoryMock;
        private readonly Mock<IDapperWrapper> _dapperWrapperMock;
        private readonly Mock<ILogger<CustomerRepository>> _loggerMock;
        private readonly CustomerRepository _customerRepository;

        public CustomerRepositoryTests()
        {
            _dbConnectionFactoryMock = new Mock<IDbConnectionFactory>();
            _dapperWrapperMock = new Mock<IDapperWrapper>();
            _loggerMock = new Mock<ILogger<CustomerRepository>>();
            _customerRepository = new CustomerRepository(_dbConnectionFactoryMock.Object, _dapperWrapperMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnTrue_WhenCustomerExists()
        {
            // Arrange
            var cpf = "12345678900";
            var connectionMock = new Mock<IDbConnection>();
            _dbConnectionFactoryMock.Setup(f => f.CreateConnection()).Returns(connectionMock.Object);
            _dapperWrapperMock.Setup(d => d.ExecuteScalarAsync<int>(connectionMock.Object, It.IsAny<CommandDefinition>()))
                .ReturnsAsync(1);

            // Act
            var result = await _customerRepository.ExistsAsync(cpf);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnFalse_WhenCustomerDoesNotExist()
        {
            // Arrange
            var cpf = "12345678900";
            var connectionMock = new Mock<IDbConnection>();
            _dbConnectionFactoryMock.Setup(f => f.CreateConnection()).Returns(connectionMock.Object);
            _dapperWrapperMock.Setup(d => d.ExecuteScalarAsync<int>(connectionMock.Object, It.IsAny<CommandDefinition>()))
                .ReturnsAsync(0);

            // Act
            var result = await _customerRepository.ExistsAsync(cpf);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task AddAsync_ShouldAddCustomer()
        {
            // Arrange
            var customer = new Customer { Id = Guid.NewGuid(), Name = "John Doe", Cpf = "12345678900", CreditLimit = 1000 };
            var connectionMock = new Mock<IDbConnection>();
            _dbConnectionFactoryMock.Setup(f => f.CreateConnection()).Returns(connectionMock.Object);

            // Act
            await _customerRepository.AddAsync(customer);

            // Assert
            _dapperWrapperMock.Verify(d => d.ExecuteAsync(connectionMock.Object, It.IsAny<CommandDefinition>()), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnCustomers()
        {
            // Arrange
            var customers = new List<Customer> { new Customer { Id = Guid.NewGuid(), Name = "John Doe", Cpf = "12345678900", CreditLimit = 1000 } };
            var connectionMock = new Mock<IDbConnection>();
            _dbConnectionFactoryMock.Setup(f => f.CreateConnection()).Returns(connectionMock.Object);
            _dapperWrapperMock.Setup(d => d.QueryAsync<Customer>(connectionMock.Object, It.IsAny<CommandDefinition>()))
                .ReturnsAsync(customers);

            // Act
            var result = await _customerRepository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetCustomerByIdAsync_ShouldReturnCustomer_WhenCustomerExists()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var customer = new Customer { Id = customerId, Name = "John Doe", Cpf = "12345678900", CreditLimit = 1000 };
            var connectionMock = new Mock<IDbConnection>();
            _dbConnectionFactoryMock.Setup(f => f.CreateConnection()).Returns(connectionMock.Object);
            _dapperWrapperMock.Setup(d => d.QuerySingleOrDefaultAsync<Customer>(connectionMock.Object, It.IsAny<CommandDefinition>()))
                .ReturnsAsync(customer);

            // Act
            var result = await _customerRepository.GetCustomerByIdAsync(customerId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(customerId, result.Id);
        }

        [Fact]
        public async Task GetCustomerByIdAsync_ShouldReturnNull_WhenCustomerDoesNotExist()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var connectionMock = new Mock<IDbConnection>();
            _dbConnectionFactoryMock.Setup(f => f.CreateConnection()).Returns(connectionMock.Object);
            _dapperWrapperMock.Setup(d => d.QuerySingleOrDefaultAsync<Customer?>(connectionMock.Object, It.IsAny<CommandDefinition>()))
                .ReturnsAsync((Customer?)null);

            // Act
            var result = await _customerRepository.GetCustomerByIdAsync(customerId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateCustomerAsync_ShouldUpdateCustomer()
        {
            // Arrange
            var customer = new Customer { Id = Guid.NewGuid(), Name = "John Doe", Cpf = "12345678900", CreditLimit = 1000 };
            var connectionMock = new Mock<IDbConnection>();
            _dbConnectionFactoryMock.Setup(f => f.CreateConnection()).Returns(connectionMock.Object);

            // Act
            await _customerRepository.UpdateCustomerAsync(customer);

            // Assert
            _dapperWrapperMock.Verify(d => d.ExecuteAsync(connectionMock.Object, It.IsAny<CommandDefinition>()), Times.Once);
        }
    }
}
