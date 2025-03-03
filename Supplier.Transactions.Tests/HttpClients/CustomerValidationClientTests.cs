using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Supplier.Transactions.HttpClients;
using Supplier.Transactions.HttpClients.Dto;
using Supplier.Transactions.Models;
using System.Net;

namespace Supplier.Transactions.Tests.HttpClients
{
    public class CustomerValidationClientTests
    {
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly Mock<ILogger<CustomerValidationClient>> _mockLogger;
        private readonly HttpClient _httpClient;
        private readonly CustomerValidationClient _client;
        private readonly string _token;

        public CustomerValidationClientTests()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _mockLogger = new Mock<ILogger<CustomerValidationClient>>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("http://test.com")
            };
            _client = new CustomerValidationClient(_httpClient, _mockLogger.Object);
            _token = "test-token";
        }

        [Fact]
        public async Task ValidateCustomerAsync_ReturnsValidationResult_WhenResponseIsSuccessful()
        {
            // Arrange
            var transactionRequest = new TransactionRequest
            {
                CustomerId = Guid.NewGuid(),
                Amount = 100
            };
            var validationResult = new CustomerValidationResultDto { IsValid = true, Message = "Valid" };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"IsValid\":true,\"Message\":\"Valid\"}")
                });

            // Act
            var result = await _client.ValidateCustomerAsync(transactionRequest, _token);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsValid);
            Assert.Equal("Valid", result.Message);
        }

        [Fact]
        public async Task ValidateCustomerAsync_ReturnsValidationError_WhenHttpRequestExceptionOccurs()
        {
            // Arrange
            var transactionRequest = new TransactionRequest
            {
                CustomerId = Guid.NewGuid(),
                Amount = 100
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException());

            // Act
            var result = await _client.ValidateCustomerAsync(transactionRequest, _token);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal("Validation error", result.Message);
        }

        [Fact]
        public async Task ValidateCustomerAsync_ReturnsUnexpectedError_WhenExceptionOccurs()
        {
            // Arrange
            var transactionRequest = new TransactionRequest
            {
                CustomerId = Guid.NewGuid(),
                Amount = 100
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exception());

            // Act
            var result = await _client.ValidateCustomerAsync(transactionRequest, _token);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal("Unexpected error", result.Message);
        }
    }
}
