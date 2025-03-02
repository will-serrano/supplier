using Supplier.Contracts.Transactions.Interfaces;

namespace Supplier.Contracts.Transactions.Requests
{
    public class TransactionRequestMessageData : ITransactionMessageData
    {
        public decimal Amount { get; set; }
        public Guid CustomerId { get; set; }
        public Guid TransactionId { get; set; }
    }
}
