using Microsoft.Data.Sqlite;
using Serilog;
using Supplier.Customers.Configuration.Interfaces;
using Supplier.Customers.Helper;
using System.Data;

namespace Supplier.Customers.Configuration
{
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly IConfiguration _configuration;

        public DbConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IDbConnection? CreateConnection()
        {
            try
            {
                string connectionString = ConnectionStringHelper.GetSqliteConnectionString(_configuration);
                Log.Information("String de conexão configurada: {ConnectionString}", connectionString);
                return new SqliteConnection(connectionString);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Erro ao obter a string de conexão");
                return null;
            }
        }
    }
}
