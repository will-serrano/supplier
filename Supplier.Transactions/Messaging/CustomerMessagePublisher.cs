using Rebus.Bus;
using Supplier.Contracts.Transactions;

namespace Supplier.Transactions.Messaging
{
    public class CustomerMessagePublisher
    {
        private readonly IBus _bus;

        public CustomerMessagePublisher(IBus bus)
        {
            _bus = bus;
        }

        public async Task Send(MessageWrapper mensagem)
        {
            // Envia a mensagem para a fila da API Transactions
            await _bus.Advanced.Routing.Send(RoutingKeys.TransactionsToCustomers, mensagem);
        }
    }
}
