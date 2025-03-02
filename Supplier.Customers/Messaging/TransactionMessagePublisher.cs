using Rebus.Bus;
using Supplier.Contracts.Transactions;

namespace Supplier.Customers.Messaging
{
    public class TransactionMessagePublisher
    {
        private readonly IBus _bus;

        public TransactionMessagePublisher(IBus bus)
        {
            _bus = bus;
        }

        public async Task Send(MessageWrapper mensagem)
        {
            // Envia a mensagem para a fila da API Transactions
            await _bus.Advanced.Routing.Send(RoutingKeys.CustomersToTransactions, mensagem);
        }
    }
}
