using Dapper;
using Supplier.Customers.Configuration.Interfaces;
using Supplier.Customers.Models;
using Supplier.Customers.Repositories.Interfaces;

namespace Supplier.Customers.Repositories
{
    /// <summary>
    /// Repository for managing customer data.
    /// </summary>
    public class CustomerRepository : ICustomerRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        private readonly IDapperWrapper _dapperWrapper;
        private readonly ILogger<CustomerRepository> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerRepository"/> class.
        /// </summary>
        /// <param name="dbConnectionFactory">The database connection factory.</param>
        /// <param name="dapperWrapper">The Dapper wrapper.</param>
        /// <param name="logger">The logger instance.</param>
        public CustomerRepository(IDbConnectionFactory dbConnectionFactory, IDapperWrapper dapperWrapper, ILogger<CustomerRepository> logger)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _dapperWrapper = dapperWrapper;
            _logger = logger;
        }

        /// <summary>
        /// Checks if a customer with the specified CPF exists.
        /// </summary>
        /// <param name="cpf">The CPF of the customer.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the customer exists.</returns>
        public async Task<bool> ExistsAsync(string cpf)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            if (connection == null)
            {
                _logger.LogError("Failed to create a connection to the database.");
                return false;
            }
            var command = new CommandDefinition("SELECT COUNT(1) FROM Customers WHERE Cpf = @Cpf", new { Cpf = cpf });
            var result = await _dapperWrapper.ExecuteScalarAsync<int>(connection, command);
            return result > 0;
        }

        /// <summary>
        /// Adds a new customer to the database.
        /// </summary>
        /// <param name="customer">The customer to add.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task AddAsync(Customer customer)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            if (connection == null)
            {
                _logger.LogError("Failed to create a connection to the database.");
                return;
            }
            var command = new CommandDefinition("INSERT INTO Customers (Id, Name, Cpf, CreditLimit) VALUES (@Id, @Name, @Cpf, @CreditLimit)", customer);
            await _dapperWrapper.ExecuteAsync(connection, command);
        }

        /// <summary>
        /// Retrieves all customers from the database.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of customers.</returns>
        public async Task<IEnumerable<Customer>?> GetAllAsync()
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            if (connection == null)
            {
                _logger.LogError("Failed to create a connection to the database.");
                return null;
            }
            var command = new CommandDefinition("SELECT Id, Name, Cpf, CreditLimit FROM Customers");
            return await _dapperWrapper.QueryAsync<Customer>(connection, command);
        }

        /// <summary>
        /// Retrieves a customer by their ID.
        /// </summary>
        /// <param name="customerId">The ID of the customer.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the customer if found; otherwise, null.</returns>
        public async Task<Customer?> GetCustomerByIdAsync(Guid customerId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            if (connection == null)
            {
                _logger.LogError("Failed to create a connection to the database.");
                return null;
            }
            var command = new CommandDefinition("SELECT Id, Name, Cpf, CreditLimit FROM Customers WHERE Id = @Id", new { Id = customerId });
            return await _dapperWrapper.QuerySingleOrDefaultAsync<Customer>(connection, command);
        }

        /// <summary>
        /// Updates an existing customer in the database.
        /// </summary>
        /// <param name="customer">The customer to update.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task UpdateCustomerAsync(Customer customer)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            if (connection == null)
            {
                _logger.LogError("Failed to create a connection to the database.");
                return;
            }
            var command = new CommandDefinition("UPDATE Customers SET Name = @Name, Cpf = @Cpf, CreditLimit = @CreditLimit WHERE Id = @Id", customer);
            await _dapperWrapper.ExecuteAsync(connection, command);
        }
    }
}
