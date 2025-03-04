using System.Net.Http.Headers;

namespace Supplier.Transactions.HttpClients
{
    /// <summary>
    /// A handler for HTTP client that adds an authentication token to the request headers.
    /// </summary>
    public class AuthenticatedHttpClientHandler : DelegatingHandler
    {
        private readonly ILogger<AuthenticatedHttpClientHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticatedHttpClientHandler"/> class.
        /// </summary>
        /// <param name="logger">The logger to log information and warnings.</param>
        public AuthenticatedHttpClientHandler(ILogger<AuthenticatedHttpClientHandler> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Sends an HTTP request with an authentication token in the headers.
        /// </summary>
        /// <param name="request">The HTTP request message to send.</param>
        /// <param name="token">The authentication token to add to the request headers.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>The HTTP response message.</returns>
        public async Task<HttpResponseMessage> ProtectedSendAsync(HttpRequestMessage request, string token, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Sending HTTP request to {Url}", request.RequestUri);

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                _logger.LogInformation("Added Bearer token to the request headers.");
            }
            else
            {
                _logger.LogWarning("No token provided.");
            }

            var response = await base.SendAsync(request, cancellationToken);

            _logger.LogInformation("Received HTTP response with status code {StatusCode}", response.StatusCode);

            return response;
        }

        /// <summary>
        /// Sends an HTTP request with an authentication token in the headers.
        /// </summary>
        /// <param name="request">The HTTP request message to send.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>The HTTP response message.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // This method can remain empty or call the base implementation if needed.
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
