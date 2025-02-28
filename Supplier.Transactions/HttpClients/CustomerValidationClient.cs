using Serilog;
using Supplier.Transactions.HttpClients.Dto;
using Supplier.Transactions.HttpClients.Interfaces;
using Supplier.Transactions.Models;

namespace Supplier.Transactions.HttpClients
{
    public class CustomerValidationClient : ICustomerValidationClient
    {
        private readonly HttpClient _httpClient;

        public CustomerValidationClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CustomerValidationResultDto> ValidateCustomerAsync(TransactionRequest transactionRequest)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/customers/{transactionRequest.CustomerId}/validate/{transactionRequest.Amount}");
                response.EnsureSuccessStatusCode(); // Lança exceção se o status não for sucesso

                var result = await response.Content.ReadFromJsonAsync<CustomerValidationResultDto>();
                return result ?? new CustomerValidationResultDto { IsValid = false, Message = "Resposta vazia" };
            }
            catch (HttpRequestException ex)
            {
                Log.Error(ex, "Erro ao chamar a API de validação para o cliente {CustomerId}", transactionRequest.CustomerId);
                // Você pode optar por retornar um resultado indicando falha ou relançar a exceção
                return new CustomerValidationResultDto { IsValid = false, Message = "Erro na validação" };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Erro inesperado na validação do cliente {CustomerId}", transactionRequest.CustomerId);
                return new CustomerValidationResultDto { IsValid = false, Message = "Erro inesperado" };
            }
        }
    }
}
