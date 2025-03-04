using Microsoft.Extensions.Caching.Memory;
using Rebus.Bus;
using Rebus.Handlers;
using Supplier.Contracts.Transactions;
using Supplier.Contracts.Transactions.Requests;
using Supplier.Contracts.Transactions.Responses;
using Supplier.Customers.Repositories.Interfaces;

namespace Supplier.Customers.Messaging
{
    /// <summary>
    /// Handles customer messages.
    /// </summary>
    public class CustomerMessageHandler : IHandleMessages<MessageWrapper>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IBus _bus;
        private readonly ILogger<CustomerMessageHandler> _logger;
        private readonly IMemoryCache _customerCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerMessageHandler"/> class.
        /// </summary>
        /// <param name="customerRepository">The customer repository.</param>
        /// <param name="bus">The bus.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="customerCache">Cache.</param>
        public CustomerMessageHandler(ICustomerRepository customerRepository, IBus bus, ILogger<CustomerMessageHandler> logger, IMemoryCache customerCache)
        {
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _customerCache = customerCache ?? throw new ArgumentNullException(nameof(customerCache));
        }

        /// <summary>
        /// Handles the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public async Task Handle(MessageWrapper message)
        {
            _logger.LogInformation("Message received: {@Message}", message);

            if (message.Data == null)
            {
                _logger.LogError("Message received with empty data.");
                return;
            }

            if (message.Data is not TransactionRequestMessageData transactionData)
            {
                _logger.LogError("Failed to convert message to TransactionRequestMessageData.");
                return;
            }

            var customer = await _customerRepository.GetCustomerByIdAsync(transactionData.CustomerId);
            if (customer == null)
            {
                _logger.LogError("Customer not found: {CustomerId}", transactionData.CustomerId);
                await SendFailureResponse(transactionData.TransactionId, "Customer not found.");
                return;
            }

            if (!customer.TryReduceCreditLimit(transactionData.Amount, out var newLimit))
            {
                _logger.LogError("Failed to update customer limit {CustomerId}", transactionData.CustomerId);
                await SendFailureResponse(transactionData.TransactionId, "Failed to update customer limit.", customer.CreditLimit);
                return;
            }

            _customerCache.Remove("customers"); // Invalidate cache when a new customer is updated
            _logger.LogInformation("Cleaning cache for customers");

            await _customerRepository.UpdateCustomerAsync(customer);
            _logger.LogInformation("Customer limit {CustomerId} updated to {NewLimit}", customer.Id, customer.CreditLimit);

            await SendSuccessResponse(transactionData.TransactionId, customer.CreditLimit);
        }

        /// <summary>
        /// Sends a failure response to the transactions service.
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="message"></param>
        /// <param name="newLimit"></param>
        /// <returns></returns>
        private async Task SendFailureResponse(Guid transactionId, string message, decimal newLimit = 0)
        {
            _logger.LogError("Sending failure response for TransactionId: {TransactionId}, Message: {Message}, NewLimit: {NewLimit}", transactionId, message, newLimit);

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
                Data = response,
                Type = Contracts.Transactions.Enums.MessageType.TransactionResponseMessageData
            };

            await _bus.Advanced.Routing.Send(RoutingKeys.CustomersToTransactions, messageToSend);
        }

        /// <summary>
        /// Sends a success response to the transactions service.
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="newLimit"></param>
        /// <returns></returns>
        private async Task SendSuccessResponse(Guid transactionId, decimal newLimit)
        {
            _logger.LogInformation("Sending success response for TransactionId: {TransactionId}, NewLimit: {NewLimit}", transactionId, newLimit);

            var responseData = new TransactionResponseMessageData
            {
                TransactionId = transactionId,
                IsSuccess = true,
                NewLimit = newLimit,
                Message = "Limit updated successfully."
            };

            var messageToSend = new MessageWrapper
            {
                Version = "V1",
                Data = responseData,
                Type = Contracts.Transactions.Enums.MessageType.TransactionResponseMessageData
            };

            await _bus.Advanced.Routing.Send(RoutingKeys.CustomersToTransactions, messageToSend);
        }
    }
}
