using FluentMigrator;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Supplier.Auth.Migrations
{
    /// <summary>
    /// Migration class to create authentication tables.
    /// </summary>
    [Migration(202503031706)]
    public class M202503031706_CreateAuthTables : Migration
    {
        /// <summary>
        /// Defines the operations to be performed when applying the migration.
        /// </summary>
        public override void Up()
        {
            // Cria a tabela Users já com ID como TEXT (para suportar SQLite)
            Create.Table("Users")
                .WithColumn("Id").AsString(36).PrimaryKey()
                .WithColumn("Email").AsString(255).NotNullable()
                .WithColumn("PasswordHash").AsString(255).NotNullable();

            // Cria a tabela Roles
            Create.Table("Roles")
                .WithColumn("Id").AsString(36).PrimaryKey()
                .WithColumn("Name").AsString(255).NotNullable()
                .WithColumn("Description").AsString(1000).NotNullable();

            // Cria a tabela UserRoles
            Create.Table("UserRoles")
                .WithColumn("UserId").AsString(36).NotNullable()
                .WithColumn("RoleId").AsString(36).NotNullable();

        }

        /// <summary>
        /// Defines the operations to be performed when rolling back the migration.
        /// </summary>
        public override void Down()
        {
            Delete.Table("UserRoles");
            Delete.Table("Roles");
            Delete.Table("Users");
        }
    }
}
