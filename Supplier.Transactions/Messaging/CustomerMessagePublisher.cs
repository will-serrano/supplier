using Rebus.Bus;
using Supplier.Transactions.Messaging.Contracts;

namespace Supplier.Transactions.Messaging
{
    public class CustomerMessagePublisher
    {
        private readonly IBus _bus;

        public CustomerMessagePublisher(IBus bus)
        {
            _bus = bus;
        }

        public async Task Send(TransactionMessage mensagem)
        {
            // Envia a mensagem para a fila da API Transactions
            await _bus.Advanced.Routing.Send(RoutingKeys.TransactionsToCustomers, mensagem);
        }
    }
}
