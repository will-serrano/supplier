using Microsoft.Data.Sqlite;
using Serilog;
using Supplier.Customers.Configuration.Interfaces;
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
            string dbFolder = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())?.FullName ?? string.Empty, "Supplier.DB");

            if (!Directory.Exists(dbFolder))
            {
                Directory.CreateDirectory(dbFolder);
                Log.Information("Diretório criado! => [{Path}]", dbFolder);
            }

            // Obtém o nome do banco do appsettings.json
            string? dbName = _configuration["DatabaseSettings:DatabaseName"];
            if (string.IsNullOrEmpty(dbName))
            {
                Log.Error("Database name not found in appsettings.json");
                return null;
            }

            string dbPath = Path.Combine(dbFolder, dbName);

            // Substitui a string de conexão com o caminho correto do banco
            string connectionString = _configuration.GetConnectionString("DefaultConnection")?.Replace("Data Source=database.sqlite", $"Data Source={dbPath}") ?? string.Empty;

            if (string.IsNullOrEmpty(connectionString))
            {
                Log.Error("String de conexão não encontrada no appsettings.json");
                return null;
            }

            Log.Information("String de conexão configurada: {ConnectionString}", connectionString);

            return new SqliteConnection(connectionString);
        }
    }
}
