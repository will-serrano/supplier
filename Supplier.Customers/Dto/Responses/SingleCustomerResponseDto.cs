using Supplier.Customers.Models;

namespace Supplier.Customers.Dto.Responses
{
    public class SingleCustomerResponseDto : CustomerResponseBase
    {
        public Guid IdCliente { get; }

        public SingleCustomerResponseDto(Customer customer)
            : base()
        {
            IdCliente = customer.Id;
        }
    }
}
