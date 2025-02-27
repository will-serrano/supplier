namespace Supplier.Customers.Dto.Requests
{
    public class CustomerRequestDto
    {
        public string? Name { get; set; }
        public string? Cpf { get; set; }
        public decimal CreditLimit { get; set; }
    }
}
