namespace Supplier.Transactions.Messaging
{
    /// <summary>
    /// Provides routing keys for messaging between customers and transactions.
    /// </summary>
    public static class RoutingKeys
    {
        /// <summary>
        /// Routing key for messages from customers to transactions.
        /// </summary>
        public const string CustomersToTransactions = "customers.to.transactions";

        /// <summary>
        /// Routing key for messages from transactions to customers.
        /// </summary>
        public const string TransactionsToCustomers = "transactions.to.customers";
    }
}
