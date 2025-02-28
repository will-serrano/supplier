using Supplier.Transactions.Enums;

namespace Supplier.Transactions.Models
{
    public class TransactionRequest
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public decimal Amount { get; set; }
        public TransactionStatus Status { get; set; }
        public Guid CustomerId { get; set; }
        public bool CustomerBlocked { get; set; }
        public Guid TransactionId { get; set; }
        public string RequestedBy { get; set; }
        public DateTime RequestedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Detail { get; set; }
    }
}
