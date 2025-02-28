using Supplier.Customers.Models;

namespace Supplier.Customers.Repositories.Interfaces
{
    public interface ICustomerRepository
    {
        Task<bool> ExistsAsync(string cpf);
        Task AddAsync(Customer customer);
        Task<IEnumerable<Customer>> GetAllAsync();
        Task<Customer?> GetCustomerByIdAsync(Guid customerId);
        Task UpdateCustomerAsync(Customer customer);
    }
}
