using Supplier.Customers.Mappers.Interfaces;
using Supplier.Customers.Models;

namespace Supplier.Customers.Dto.Responses
{
    public class MultipleCustomersResponseDto : CustomerResponseBase
    {
        public IEnumerable<CustomerResponseDto> Customers { get; }

        public MultipleCustomersResponseDto(IEnumerable<CustomerResponseDto> multipleCustomers)
             : base()
        {
            Customers = multipleCustomers;
        }
    }
}
