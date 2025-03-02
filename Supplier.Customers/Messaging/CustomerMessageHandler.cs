using Rebus.Bus;
using Rebus.Handlers;
using Supplier.Contracts.Transactions;
using Supplier.Contracts.Transactions.Requests;
using Supplier.Contracts.Transactions.Responses;
using Supplier.Customers.Repositories.Interfaces;

namespace Supplier.Customers.Messaging
{
    public class CustomerMessageHandler : IHandleMessages<MessageWrapper>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IBus _bus;
        private readonly ILogger<CustomerMessageHandler> _logger;

        public CustomerMessageHandler(ICustomerRepository customerRepository, IBus bus, ILogger<CustomerMessageHandler> logger)
        {
            _customerRepository = customerRepository;
            _bus = bus;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(MessageWrapper message)
        {
            _logger.LogInformation("Mensagem recebida: {@Message}", message);

            if (message.Data == null)
            {
                _logger.LogError("Mensagem recebida com dados vazios.");
                return;
            }

            if (message.Data is not TransactionRequestMessageData transactionData)
            {
                _logger.LogError("Falha ao converter a mensagem para TransactionRequestMessageData.");
                return;
            }

            var customer = await _customerRepository.GetCustomerByIdAsync(transactionData.CustomerId);
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

        private async Task SendFailureResponse(Guid transactionId, string message, decimal newLimit = 0)
        {
            var response = new TransactionResponseMessageData
            {
                TransactionId = transactionId,
                IsSuccess = false,
                NewLimit = newLimit,
                Message = message
            };

            var messageToSend = new MessageWrapper
            {
                Version = "V1",
                Data = response
            };

            await _bus.Advanced.Routing.Send(RoutingKeys.CustomersToTransactions, messageToSend);
        }

        private async Task SendSuccessResponse(Guid transactionId, decimal newLimit)
        {
            var responseData = new TransactionResponseMessageData
            {
                TransactionId = transactionId,
                IsSuccess = true,
                NewLimit = newLimit,
                Message = "Limite atualizado com sucesso."
            };

            var messageToSend = new MessageWrapper
            {
                Version = "V1",
                Data = responseData
            };

            await _bus.Advanced.Routing.Send(RoutingKeys.CustomersToTransactions, messageToSend);
        }
    }
}
