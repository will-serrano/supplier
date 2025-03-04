namespace Supplier.Customers.Dto.Responses
{
    public class CustomerResponseDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Cpf { get; set; }
        public decimal CreditLimit { get; set; }
    }
}
