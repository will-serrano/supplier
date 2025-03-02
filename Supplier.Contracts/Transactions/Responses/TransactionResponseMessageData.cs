using Supplier.Contracts.Transactions.Interfaces;

namespace Supplier.Contracts.Transactions.Responses
{
    public class TransactionResponseMessageData : ITransactionMessageData
    {
        public Guid TransactionId { get; set; }
        public bool IsSuccess { get; set; }
        public decimal NewLimit { get; set; }
        public string? Message { get; set; }
    }
}
