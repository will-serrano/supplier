using System.Net.Http.Headers;

namespace Supplier.Transactions.HttpClients
{
    /// <summary>
    /// A handler for HTTP client that adds an authentication token to the request headers.
    /// </summary>
    public class AuthenticatedHttpClientHandler : DelegatingHandler
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthenticatedHttpClientHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticatedHttpClientHandler"/> class.
        /// </summary>
        /// <param name="configuration">The configuration to retrieve the token from.</param>
        /// <param name="logger">The logger to log information and warnings.</param>
        public AuthenticatedHttpClientHandler(IConfiguration configuration, ILogger<AuthenticatedHttpClientHandler> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<HttpResponseMessage> ProtectedSendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return await SendAsync(request, cancellationToken);
        }

        /// <summary>
        /// Sends an HTTP request with an authentication token in the headers.
        /// </summary>
        /// <param name="request">The HTTP request message to send.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>The HTTP response message.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Sending HTTP request to {Url}", request.RequestUri);

            // Read the token from appsettings (or another service if dynamic)
            var token = _configuration["CustomerApi:Token"];
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                _logger.LogInformation("Added Bearer token to the request headers.");
            }
            else
            {
                _logger.LogWarning("No token found in configuration.");
            }

            var response = await base.SendAsync(request, cancellationToken);

            _logger.LogInformation("Received HTTP response with status code {StatusCode}", response.StatusCode);

            return response;
        }
    }
}
