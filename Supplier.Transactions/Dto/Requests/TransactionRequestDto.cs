namespace Supplier.Transactions.Dto.Requests
{
    public class TransactionRequestDto
    {
        public string? CustomerId { get; set; }
        public decimal? Amount { get; set; }
        public string? UserId { get; internal set; }
    }
}
