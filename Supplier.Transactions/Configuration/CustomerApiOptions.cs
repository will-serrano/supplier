namespace Supplier.Transactions.Configuration
{
    public class CustomerApiOptions
    {
        public string? BaseAddress { get; set; }
        public int TimeoutSeconds { get; set; }
        public int RetryCount { get; set; }
        public int RetryDelaySeconds { get; set; }
    }
}
