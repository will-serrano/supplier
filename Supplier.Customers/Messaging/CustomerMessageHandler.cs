using Rebus.Bus;
using Rebus.Handlers;
using Supplier.Customers.Messaging.Contracts;
using Supplier.Customers.Models;
using Supplier.Customers.Repositories.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Supplier.Customers.Messaging
{
    public class CustomerMessageHandler : IHandleMessages<CustomerMessage>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IBus _bus;
        private readonly ILogger<CustomerMessageHandler> _logger;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public CustomerMessageHandler(ICustomerRepository customerRepository, IBus bus, ILogger<CustomerMessageHandler> logger)
        {
            _customerRepository = customerRepository;
            _bus = bus;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(CustomerMessage message)
        {
            _logger.LogInformation("Mensagem recebida: {@Message}", message);

            if (string.IsNullOrWhiteSpace(message.Data))
            {
                _logger.LogError("Mensagem recebida com dados vazios.");
                return;
            }

            if (!TryDeserializeMessage(message.Data, out var transactionData))
            {
                _logger.LogError("Falha ao desserializar a mensagem: {Data}", message.Data);
                return;
            }

            var customer = await _customerRepository.GetCustomerByIdAsync(transactionData!.CustomerId);

            if (customer == null)
            {
                _logger.LogError("Cliente não encontrado: {CustomerId}", transactionData.CustomerId);
                await SendFailureResponse(transactionData.TransactionId, "Cliente não encontrado.");
                return;
            }

            if (!customer.TryReduceCreditLimit(transactionData.Amount, out var newLimit))
            {
                _logger.LogError("Falha ao atualizar o limite do cliente {CustomerId}", transactionData.CustomerId);
                await SendFailureResponse(transactionData.TransactionId, "Falha ao atualizar o limite do cliente.", customer.CreditLimit);
                return;
            }

            await _customerRepository.UpdateCustomerAsync(customer);
            _logger.LogInformation("Limite do cliente {CustomerId} atualizado para {NewLimit}", customer.Id, customer.CreditLimit);

            await SendSuccessResponse(transactionData.TransactionId, customer.CreditLimit);
        }

        private static bool TryDeserializeMessage(string json, out TransactionMessageData? data)
        {
            try
            {
                data = System.Text.Json.JsonSerializer.Deserialize<TransactionMessageData>(json, _jsonOptions);
                return data != null;
            }
            catch (JsonException)
            {
                data = null;
                return false;
            }
        }

        private async Task SendFailureResponse(Guid transactionId, string message, decimal newLimit = 0)
        {
            var response = new CustomerUpdateResponseMessageData
            {
                TransactionId = transactionId,
                IsSuccess = false,
                NewLimit = newLimit,
                Message = message
            };

            await _bus.Advanced.Routing.Send(RoutingKeys.CustomersToTransactions, response);
        }

        private async Task SendSuccessResponse(Guid transactionId, decimal newLimit)
        {
            var responseData = new CustomerUpdateResponseMessageData
            {
                TransactionId = transactionId,
                IsSuccess = true,
                NewLimit = newLimit,
                Message = "Limite atualizado com sucesso."
            };

            var messageToSend = new TransactionMessage
            {
                Version = "V1",
                Data = JsonSerializer.Serialize(responseData, _jsonOptions)
            };

            await _bus.Advanced.Routing.Send(RoutingKeys.CustomersToTransactions, messageToSend);
        }
    }
}
