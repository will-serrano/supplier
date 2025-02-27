using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Serilog;
using Supplier.Auth.Configuration.Interfaces;
using System.Data;

namespace Supplier.Auth.Configuration
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

            // Obtém o caminho do banco do appsettings.json
            string dbPath = Path.Combine(dbFolder, "Supplier.Auth.db");

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