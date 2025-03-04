namespace Supplier.Customers.Models
{
    public class Customer
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? Name { get; set; }
        public string? Cpf { get; set; }
        public decimal CreditLimit { get; set; }

        public bool TryReduceCreditLimit(decimal amount, out decimal creditLimit)
        {
            creditLimit = this.CreditLimit;

            if (amount < 0)
            {
                return false;
            }

            if (this.CreditLimit - amount < 0)
            {
                return false;
            }

            this.CreditLimit -= amount;
            creditLimit = this.CreditLimit;
            return true;
        }
    }
}
