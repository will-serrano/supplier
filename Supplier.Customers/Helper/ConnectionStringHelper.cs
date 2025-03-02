namespace Supplier.Customers.Helper
{
    public static class ConnectionStringHelper
    {
        public static string GetSqliteConnectionString(IConfiguration configuration)
        {
            string dbFolder = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())?.FullName ?? string.Empty, "Supplier.DB");

            if (!Directory.Exists(dbFolder))
                Directory.CreateDirectory(dbFolder);

            // Obtém o nome do banco do appsettings.json
            string? dbName = configuration["DatabaseSettings:DatabaseName"];
            if (string.IsNullOrEmpty(dbName))
                throw new Exception("Database name not found in appsettings.json");

            string dbPath = Path.Combine(dbFolder, dbName);
            string connectionString = configuration.GetConnectionString("DefaultConnection")?
                .Replace("Data Source=database.sqlite", $"Data Source={dbPath}") ?? string.Empty;

            if (string.IsNullOrEmpty(connectionString))
                throw new Exception("String de conexão não encontrada no appsettings.json");

            return connectionString;
        }
    }
}
