using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Supplier.Transactions.Controllers;
using Supplier.Transactions.Dto.Requests;
using Supplier.Transactions.Dto.Responses;
using Supplier.Transactions.Services.Interfaces;
using System.Security.Claims;

namespace Supplier.Transactions.Tests.Controllers
{
    public class TransactionControllerTests
    {
        private readonly Mock<ITransactionRequestService> _mockTransactionService;
        private readonly TransactionController _controller;
        private readonly ILogger<TransactionController> _logger;

        public TransactionControllerTests()
        {
            _mockTransactionService = new Mock<ITransactionRequestService>();
            _logger = new Mock<ILogger<TransactionController>>().Object;
            _controller = new TransactionController(_mockTransactionService.Object, _logger);
        }

        [Fact]
        public async Task RequestTransaction_ReturnsBadRequest_WhenRequestIsNull()
        {
            // Act
            var result = await _controller.RequestTransaction(null!);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Request cannot be null", badRequestResult.Value);
        }

        [Fact]
        public async Task RequestTransaction_ReturnsUnauthorized_WhenUserIdIsNull()
        {
            // Arrange
            var request = new TransactionRequestDto();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            };

            // Act
            var result = await _controller.RequestTransaction(request);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("User ID not found", unauthorizedResult.Value);
        }

        [Fact]
        public async Task RequestTransaction_ReturnsOk_WhenTransactionIsSuccessful()
        {
            // Arrange
            var request = new TransactionRequestDto();
            var userId = Guid.NewGuid().ToString();
            var response = new TransactionResponseDto { Status = "Success", TransactionId = Guid.NewGuid() };

            _mockTransactionService.Setup(service => service.RequestTransactionAsync(request))
                .ReturnsAsync(response);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                            new Claim(ClaimTypes.NameIdentifier, userId)
                    }))
                }
            };

            // Act
            var result = await _controller.RequestTransaction(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task RequestTransaction_ReturnsInternalServerError_WhenTransactionFails()
        {
            // Arrange
            var request = new TransactionRequestDto();
            var userId = Guid.NewGuid().ToString();

            _mockTransactionService.Setup(service => service.RequestTransactionAsync(request))
                .ThrowsAsync(new Exception());

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                            new Claim(ClaimTypes.NameIdentifier, userId)
                    }))
                }
            };

            // Act
            var result = await _controller.RequestTransaction(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("An internal server error occurred", statusCodeResult.Value);
        }
    }
}
