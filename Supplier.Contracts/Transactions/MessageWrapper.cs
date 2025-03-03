using Supplier.Contracts.Transactions.Enums;
using Supplier.Contracts.Transactions.Interfaces;
using System.Text.Json.Serialization;

namespace Supplier.Contracts.Transactions
{
    public class MessageWrapper
    {
        public string? Version { get; set; }
        public MessageType  Type { get; set; }

        [JsonConverter(typeof(TransactionMessageDataConverter))]
        public ITransactionMessageData? Data { get; set; }

        public MessageWrapper() { }
    }
}
