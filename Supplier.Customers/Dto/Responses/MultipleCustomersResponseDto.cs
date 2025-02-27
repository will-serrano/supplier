using Supplier.Customers.Mappers.Interfaces;
using Supplier.Customers.Models;

namespace Supplier.Customers.Dto.Responses
{
    public class MultipleCustomersResponseDto : CustomerResponseBase
    {
        public IEnumerable<CustomerResponseDto> Clientes { get; }

        public MultipleCustomersResponseDto(IEnumerable<CustomerResponseDto> multipleCustomers)
             : base()
        {
            Clientes = multipleCustomers;
        }
    }
}
