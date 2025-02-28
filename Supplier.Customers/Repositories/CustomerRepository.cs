using Dapper;
using Supplier.Customers.Models;
using Supplier.Customers.Repositories.Interfaces;
using System.Data;

namespace Supplier.Customers.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly IDbConnection _db;

        public CustomerRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<bool> ExistsAsync(string cpf)
        {
            var result = await _db.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM Customers WHERE CPF = @CPF", new { CPF = cpf });
            return result > 0;
        }

        public async Task AddAsync(Customer customer)
        {
            await _db.ExecuteAsync("INSERT INTO Customers (Id, Name, CPF, CreditLimit) VALUES (@Id, @Name, @CPF, @CreditLimit)", customer);
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            return await _db.QueryAsync<Customer>("SELECT * FROM Customers");
        }

        public async Task<Customer?> GetCustomerByIdAsync(Guid customerId)
        {
            return await _db.QuerySingleOrDefaultAsync<Customer>("SELECT * FROM Customers WHERE Id = @Id", new { Id = customerId });
        }

        public async Task UpdateCustomerAsync(Customer customer)
        {
            await _db.ExecuteAsync("UPDATE Customers SET Name = @Name, CPF = @CPF, CreditLimit = @CreditLimit WHERE Id = @Id", customer);
        }
    }
}
