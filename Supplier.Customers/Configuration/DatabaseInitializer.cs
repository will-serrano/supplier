using Dapper;
using Serilog;
using Supplier.Customers.Configuration.Interfaces;

namespace Supplier.Customers.Configuration
{
    public static class DatabaseInitializer
    {
        public static void Initialize(IDbConnectionFactory dbConnectionFactory)
        {
            using var connection = dbConnectionFactory.CreateConnection();

            if (connection == null)
            {
                Log.Error("Falha ao criar conexão com o banco de dados");
                return;
            }

            connection.Open();

            string createCustomersTable = @"
            CREATE TABLE IF NOT EXISTS Customers (
                Id TEXT PRIMARY KEY,
                Name TEXT NOT NULL,
                CPF TEXT NOT NULL UNIQUE,
                CreditLimit DECIMAL NOT NULL
            );";

            connection.Execute(createCustomersTable);
        }
    }
}
