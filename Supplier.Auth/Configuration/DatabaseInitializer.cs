using Dapper;
using Serilog;
using Supplier.Auth.Configuration.Interfaces;

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

        string createUsersTable = @"
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Email TEXT NOT NULL UNIQUE,
                PasswordHash TEXT NOT NULL,
                RefreshToken TEXT NULL,
                RefreshTokenExpiryTime DATETIME NULL
            );";

        string createRolesTable = @"
            CREATE TABLE IF NOT EXISTS Roles (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL UNIQUE
            );";

        string createUserRolesTable = @"
            CREATE TABLE IF NOT EXISTS UserRoles (
                UserId INTEGER NOT NULL,
                RoleId INTEGER NOT NULL,
                PRIMARY KEY (UserId, RoleId),
                FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
                FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE
            );";

        connection.Execute(createUsersTable);
        connection.Execute(createRolesTable);
        connection.Execute(createUserRolesTable);
    }
}
