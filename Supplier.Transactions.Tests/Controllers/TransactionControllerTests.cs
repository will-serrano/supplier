using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Supplier.Transactions.Configuration.Interfaces;
using Supplier.Transactions.Controllers;
using Supplier.Transactions.Dto.Requests;
using Supplier.Transactions.Dto.Responses;
using Supplier.Transactions.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Supplier.Transactions.Tests.Controllers
{
    public class TransactionControllerTests
    {
        private readonly Mock<ITransactionRequestService> _mockTransactionService;
        private readonly Mock<IJwtSecurityTokenHandlerWrapper> _mockTokenHandlerWrapper;
        private readonly TransactionController _controller;
        private readonly Mock<ILogger<TransactionController>> _mockLogger;

        public TransactionControllerTests()
        {
            _mockTransactionService = new Mock<ITransactionRequestService>();
            _mockTokenHandlerWrapper = new Mock<IJwtSecurityTokenHandlerWrapper>();
            _mockLogger = new Mock<ILogger<TransactionController>>();
            _controller = new TransactionController(_mockTransactionService.Object, _mockLogger.Object, _mockTokenHandlerWrapper.Object);
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
            var token = "sample-token"; // Mock token for testing

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity()),
                    Request = { Headers = { ["Authorization"] = $"Bearer {token}" } }
                }
            };

            _mockTokenHandlerWrapper.Setup(handler => handler.ReadJwtToken(token))
                .Returns(new JwtSecurityToken());

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
            var token = "sample-token"; // Mock token for testing

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };
            var jwtToken = new JwtSecurityToken(claims: claims);

            _mockTokenHandlerWrapper.Setup(handler => handler.ReadJwtToken(token)).Returns(jwtToken);

            _mockTransactionService.Setup(service => service.RequestTransactionAsync(request, token))
                .ReturnsAsync(response);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims)),
                    Request = { Headers = { ["Authorization"] = $"Bearer {token}" } }
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
            var token = "sample-token"; // Mock token for testing

            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userId)
                };
            var jwtToken = new JwtSecurityToken(claims: claims);

            _mockTokenHandlerWrapper.Setup(handler => handler.ReadJwtToken(token)).Returns(jwtToken);

            _mockTransactionService.Setup(service => service.RequestTransactionAsync(request, token))
                .ThrowsAsync(new Exception());

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims)),
                    Request = { Headers = { ["Authorization"] = $"Bearer {token}" } }
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
