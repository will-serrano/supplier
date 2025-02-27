using Supplier.Customers.Dto.Requests;
using Supplier.Customers.Models;

namespace Supplier.Customers.Filters
{
    public static class CustomerFilter
    {
        public static IEnumerable<Customer> ApplyFilters(IEnumerable<Customer> customers, CustomerRequestDto dto)
        {
            var filteredCustomers = customers.AsQueryable();

            if (!string.IsNullOrEmpty(dto.Name))
                filteredCustomers = filteredCustomers.Where(c => c.Name != null && c.Name.Contains(dto.Name, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(dto.Cpf))
                filteredCustomers = filteredCustomers.Where(c => c.Cpf == dto.Cpf);

            filteredCustomers = filteredCustomers.Where(c => c.CreditLimit >= dto.CreditLimit);

            return filteredCustomers;
        }
    }
}
