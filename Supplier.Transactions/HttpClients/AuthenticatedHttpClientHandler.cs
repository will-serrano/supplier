using System.Net.Http.Headers;

namespace Supplier.Transactions.HttpClients
{
    public class AuthenticatedHttpClientHandler : DelegatingHandler
    {
        private readonly IConfiguration _configuration;

        public AuthenticatedHttpClientHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Lê o token do appsettings (ou de outro serviço, se for dinâmico)
            var token = _configuration["CustomerApi:Token"];
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
