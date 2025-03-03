using Supplier.Contracts.Transactions.Enums;
using Supplier.Contracts.Transactions.Interfaces;

namespace Supplier.Contracts.Transactions
{
    public class MessageWrapper
    {
        public string? Version { get; set; }
        public MessageType  Type { get; set; }
        public ITransactionMessageData? Data { get; set; }

        public MessageWrapper() { }
    }
}
