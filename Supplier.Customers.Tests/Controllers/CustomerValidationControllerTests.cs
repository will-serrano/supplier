using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Supplier.Customers.Controllers;
using Supplier.Customers.Dto.Responses;
using Supplier.Customers.Services.Interfaces;

namespace Supplier.Customers.Tests.Controllers
{
    public class CustomerValidationControllerTests
    {
        private readonly Mock<ICustomerService> _mockCustomerService;
        private readonly Mock<ILogger<CustomerValidationController>> _mockLogger;
        private readonly CustomerValidationController _controller;

        public CustomerValidationControllerTests()
        {
            _mockCustomerService = new Mock<ICustomerService>();
            _mockLogger = new Mock<ILogger<CustomerValidationController>>();
            _controller = new CustomerValidationController(_mockCustomerService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task ValidateCustomer_ReturnsOk_WhenValidationSucceeds()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var amount = 100m;
            var response = new CustomerValidationResponseDto { IsValid = true };
            _mockCustomerService.Setup(service => service.ValidateCustomerAsync(customerId, amount))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.ValidateCustomer(customerId, amount);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task ValidateCustomer_ReturnsNotFound_WhenValidationFails()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var amount = 100m;
            var response = new CustomerValidationResponseDto { IsValid = false, Message = "Validation failed" };
            _mockCustomerService.Setup(service => service.ValidateCustomerAsync(customerId, amount))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.ValidateCustomer(customerId, amount);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(response, notFoundResult.Value);
        }
    }
}
