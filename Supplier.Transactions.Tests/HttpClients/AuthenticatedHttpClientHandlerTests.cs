using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Supplier.Transactions.HttpClients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Supplier.Transactions.Tests.HttpClients
{
    public class AuthenticatedHttpClientHandlerTests
    {
        private readonly Mock<ILogger<AuthenticatedHttpClientHandler>> _mockLogger;
        private readonly AuthenticatedHttpClientHandler _handler;
        private readonly HttpMessageHandler _innerHandler;

        public AuthenticatedHttpClientHandlerTests()
        {
            _mockLogger = new Mock<ILogger<AuthenticatedHttpClientHandler>>();
            _innerHandler = new Mock<HttpMessageHandler>().Object;
            _handler = new AuthenticatedHttpClientHandler(_mockLogger.Object)
            {
                InnerHandler = _innerHandler
            };
        }

        [Fact]
        public async Task SendAsync_AddsTokenToRequestHeaders_WhenTokenIsPresent()
        {
            // Arrange
            var token = "test-token";
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var mockInnerHandler = Mock.Get(_innerHandler);
            mockInnerHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // Act
            var result = await _handler.ProtectedSendAsync(request, token,  CancellationToken.None);

            // Assert
            Assert.Equal(response, result);
            Assert.NotNull(request.Headers.Authorization);
            Assert.Equal("Bearer", request.Headers.Authorization.Scheme);
            Assert.Equal(token, request.Headers.Authorization.Parameter);
        }

        [Fact]
        public async Task SendAsync_DoesNotAddTokenToRequestHeaders_WhenTokenIsNotPresent()
        {
            // Arrange
            var token = "test-token";
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var mockInnerHandler = Mock.Get(_innerHandler);
            mockInnerHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // Act
            var result = await _handler.ProtectedSendAsync(request, token, CancellationToken.None);

            // Assert
            Assert.Equal(response, result);
            Assert.Null(request.Headers.Authorization);
        }
    }
}
