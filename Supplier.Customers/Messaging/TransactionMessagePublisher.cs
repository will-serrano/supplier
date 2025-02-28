using Rebus.Bus;
using Supplier.Customers.Messaging.Contracts;

namespace Supplier.Customers.Messaging
{
    public class TransactionMessagePublisher
    {
        private readonly IBus _bus;

        public TransactionMessagePublisher(IBus bus)
        {
            _bus = bus;
        }

        public async Task Send(CustomerMessage mensagem)
        {
            // Envia a mensagem para a fila da API Transactions
            await _bus.Advanced.Routing.Send(RoutingKeys.CustomersToTransactions, mensagem);
        }
    }
}
