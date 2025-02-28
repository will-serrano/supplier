namespace Supplier.Customers.Messaging.Contracts
{
    public class CustomerUpdateResponseMessageData
    {
        public Guid TransactionId { get; set; }
        public bool IsSuccess { get; set; }
        public decimal NewLimit { get; set; }
        public string Message { get; set; }
    }
}
