namespace Supplier.Customers.Messaging.Contracts
{
    public class TransactionMessageData
    {
        public decimal Amount { get; set; }
        public Guid CustomerId { get; set; }
        public Guid TransactionId { get; internal set; }
    }
}
