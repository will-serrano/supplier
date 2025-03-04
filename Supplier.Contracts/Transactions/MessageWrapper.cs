using Supplier.Contracts.Transactions.Enums;
using Supplier.Contracts.Transactions.Interfaces;
using System.Text.Json.Serialization;

namespace Supplier.Contracts.Transactions
{
    /// <summary>
    /// Wrapper for transaction messages.
    /// </summary>
    public class MessageWrapper
    {
        /// <summary>
        /// Gets or sets the version of the message.
        /// </summary>
        public string? Version { get; set; }
        /// <summary>
        /// Gets or sets the type of the message.
        /// </summary>
        public MessageType  Type { get; set; }
        /// <summary>
        /// Gets or sets the data of the message.
        /// </summary>
        [JsonConverter(typeof(TransactionMessageDataConverter))]
        public ITransactionMessageData? Data { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageWrapper"/> class.
        /// </summary>
        public MessageWrapper() { }
    }
}
