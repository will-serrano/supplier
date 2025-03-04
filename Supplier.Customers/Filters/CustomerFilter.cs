using Supplier.Customers.Dto.Requests;
using Supplier.Customers.Models;

namespace Supplier.Customers.Filters
{
    public static class CustomerFilter
    {
        public static IEnumerable<Customer> ApplyFilters(IEnumerable<Customer> customers, CustomerRequestDto dto)
        {
            var filteredCustomers = customers.AsQueryable();

            // Filtro por Nome
            if (!string.IsNullOrEmpty(dto.Name))
                filteredCustomers = filteredCustomers.Where(c => c.Name != null && c.Name.Contains(dto.Name, StringComparison.OrdinalIgnoreCase));

            // Filtro por CPF
            if (!string.IsNullOrEmpty(dto.Cpf))
                filteredCustomers = filteredCustomers.Where(c => c.Cpf != null && c.Cpf == dto.Cpf);

            // Filtro por Limite de Crédito
            if (dto.CreditLimit > 0) // Apenas filtra se o limite de crédito for maior que 0
                filteredCustomers = filteredCustomers.Where(c => c.CreditLimit >= dto.CreditLimit);

            return filteredCustomers;
        }
    }
}
