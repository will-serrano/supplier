using Supplier.Transactions.HttpClients.Dto;
using Supplier.Transactions.HttpClients.Interfaces;
using Supplier.Transactions.Models;
using System.Net.Http.Headers;

namespace Supplier.Transactions.HttpClients
{
    /// <summary>
    /// Client for validating customers via an external API.
    /// </summary>
    public class CustomerValidationClient : ICustomerValidationClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CustomerValidationClient> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerValidationClient"/> class.
        /// </summary>
        /// <param name="httpClient">The HTTP client instance.</param>
        /// <param name="logger">The logger instance.</param>
        public CustomerValidationClient(HttpClient httpClient, ILogger<CustomerValidationClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// Validates a customer based on the provided transaction request.
        /// </summary>
        /// <param name="transactionRequest">The transaction request containing customer details.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the validation result.</returns>
        public async Task<CustomerValidationResultDto> ValidateCustomerAsync(TransactionRequest transactionRequest, string token)
        {
            try
            {
                _logger.LogInformation("Starting customer validation for CustomerId: {CustomerId}", transactionRequest.CustomerId);

                var request = new HttpRequestMessage(HttpMethod.Get, $"/api/customers/{transactionRequest.CustomerId}/validate/{transactionRequest.Amount}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);

                _logger.LogInformation("Response status code: {StatusCode}", response.StatusCode);

                response.EnsureSuccessStatusCode(); // Throws an exception if the status is not successful

                var result = await response.Content.ReadFromJsonAsync<CustomerValidationResultDto>();
                _logger.LogInformation("Customer validation completed for CustomerId: {CustomerId}", transactionRequest.CustomerId);
                return result ?? new CustomerValidationResultDto { IsValid = false, Message = "Empty response" };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error calling validation API for CustomerId: {CustomerId}", transactionRequest.CustomerId);
                // You can choose to return a result indicating failure or rethrow the exception
                return new CustomerValidationResultDto { IsValid = false, Message = "Validation error" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during customer validation for CustomerId: {CustomerId}", transactionRequest.CustomerId);
                return new CustomerValidationResultDto { IsValid = false, Message = "Unexpected error" };
            }
        }
    }
}
