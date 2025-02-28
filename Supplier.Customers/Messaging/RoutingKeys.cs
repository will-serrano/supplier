namespace Supplier.Customers.Messaging
{
    public static class RoutingKeys
    {
        public const string CustomersToTransactions = "customers.to.transactions";
        public const string TransactionsToCustomers = "transactions.to.customers";
    }
}
