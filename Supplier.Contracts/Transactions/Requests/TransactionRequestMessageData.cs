using Supplier.Contracts.Transactions.Interfaces;

namespace Supplier.Contracts.Transactions.Requests
{
    /// <summary>
    /// Represents the data transfer object for a transaction request message.
    /// </summary>
    public class TransactionRequestMessageData : ITransactionMessageData
    {
        /// <summary>
        /// Gets or sets the amount of the transaction.
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// Gets or sets the unique identifier for the customer.
        /// </summary>
        public Guid CustomerId { get; set; }
        /// <summary>
        /// Gets or sets the unique identifier for the transaction.
        /// </summary>
        public Guid TransactionId { get; set; }
    }
}
