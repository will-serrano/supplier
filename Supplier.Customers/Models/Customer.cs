namespace Supplier.Customers.Models
{
    public class Customer
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? Name { get; set; }
        public string? Cpf { get; set; }
        public decimal CreditLimit { get; set; }
    }
}
