using Rebus.Bus;
using Supplier.Contracts.Transactions;
using Supplier.Transactions.Messaging.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Supplier.Transactions.Messaging
{
    /// <summary>
    /// Publishes customer messages to the appropriate queue.
    /// </summary>
    public class CustomerMessagePublisher: ICustomerMessagePublisher
    {
        private readonly IBus _bus;
        private readonly ILogger<CustomerMessagePublisher> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerMessagePublisher"/> class.
        /// </summary>
        /// <param name="bus">The bus used for message routing.</param>
        /// <param name="logger">The logger used for logging information.</param>
        public CustomerMessagePublisher(IBus bus, ILogger<CustomerMessagePublisher> logger)
        {
            _bus = bus;
            _logger = logger;
        }

        /// <summary>
        /// Sends a message to the customer queue.
        /// </summary>
        /// <param name="mensagem">The message to be sent.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        public async Task Send(MessageWrapper mensagem)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                ReferenceHandler = ReferenceHandler.Preserve,
                Converters = { new TransactionMessageDataConverter() }
            };

            string json = JsonSerializer.Serialize(mensagem, options);
            _logger.LogInformation("Serialized message: {Json}", json);

            // Envio para o RabbitMQ
            await _bus.Advanced.Routing.Send(RoutingKeys.TransactionsToCustomers, mensagem);
        }
    }
}
