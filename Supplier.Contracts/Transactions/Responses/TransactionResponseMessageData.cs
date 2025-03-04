using Supplier.Contracts.Transactions.Interfaces;

namespace Supplier.Contracts.Transactions.Responses
{
    /// <summary>
    /// Represents the response data for a transaction message.
    /// </summary>
    public class TransactionResponseMessageData : ITransactionMessageData
    {
        /// <summary>
        /// Gets or sets the transaction ID.
        /// </summary>
        public Guid TransactionId { get; set; }
        /// <summary>
        /// Gets or sets the success status of the transaction.
        /// </summary>
        public bool IsSuccess { get; set; }
        /// <summary>
        /// Gets or sets the new limit for the customer.
        /// </summary>
        public decimal NewLimit { get; set; }
        /// <summary>
        /// Gets or sets the message for the transaction.
        /// </summary>
        public string? Message { get; set; }
    }
}
