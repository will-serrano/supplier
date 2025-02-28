namespace Supplier.Transactions.HttpClients.Dto
{
    public class CustomerValidationResultDto
    {
        public bool IsValid { get; set; }
        public string? Message { get; set; }
    }
}
