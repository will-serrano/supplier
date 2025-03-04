using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Supplier.Customers.Controllers;
using Supplier.Customers.Dto.Requests;
using Supplier.Customers.Dto.Responses;
using Supplier.Customers.Models;
using Supplier.Customers.Services.Interfaces;
using Supplier.Customers.Validators;

namespace Supplier.Customers.Tests.Controllers
{
    public class CustomerControllerTests
    {
        private readonly Mock<ICustomerService> _mockService;
        private readonly CustomerController _controller;
        private readonly ILogger<CustomerController> _logger;

        public CustomerControllerTests()
        {
            _mockService = new Mock<ICustomerService>();
            _logger = new Mock<ILogger<CustomerController>>().Object;
            _controller = new CustomerController(_mockService.Object, _logger);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenRequestIsNull()
        {
            // Act
            var result = await _controller.Create(null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponseDto>(badRequestResult.Value);
            Assert.Equal("Invalid request data.", errorResponse.ErrorDetail);
        }

        [Fact]
        public async Task Create_ReturnsOk_WhenServiceSucceeds()
        {
            // Arrange
            var request = new CustomerRequestDto { Name = "John Doe", Cpf = "12345678901", CreditLimit = 1000 };
            var response = new SingleCustomerResponseDto(new Customer { Id = Guid.NewGuid(), Name = "John Doe", Cpf = "12345678901", CreditLimit = 1000 });
            _mockService.Setup(s => s.CreateCustomerAsync(request)).ReturnsAsync(response);

            // Act
            var result = await _controller.Create(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenServiceThrowsException()
        {
            // Arrange
            var request = new CustomerRequestDto { Name = "John Doe", Cpf = "12345678901", CreditLimit = 1000 };
            _mockService.Setup(s => s.CreateCustomerAsync(request)).ThrowsAsync(new Exception("Service error"));

            // Act
            var result = await _controller.Create(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponseDto>(badRequestResult.Value);
            Assert.Equal("Service error", errorResponse.ErrorDetail);
        }

        [Fact]
        public async Task Get_ReturnsOk_WithCustomers()
        {
            // Arrange
            var customers = new MultipleCustomersResponseDto(new List<CustomerResponseDto>
            {
                new CustomerResponseDto { Id = Guid.NewGuid(), Name = "John Doe", Cpf = "12345678901", CreditLimit = 1000 }
            });
            _mockService.Setup(s => s.GetCustomersAsync(null, null, null)).ReturnsAsync(customers);

            // Act
            var result = await _controller.Get(null, null, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(customers, okResult.Value);
        }
    }
}
