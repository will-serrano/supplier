using FluentMigrator;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Supplier.Auth.Migrations
{
    [Migration(202503020106)]
    public class M202503020106_CreateRoleAndUserRoleTables : Migration
    {
        public override void Up()
        {
            // Cria a tabela Roles
            Create.Table("Roles")
                .WithColumn("Id").AsGuid().PrimaryKey()
                .WithColumn("Name").AsString(255).NotNullable()
                .WithColumn("Description").AsString(1000).NotNullable();

            // Cria a tabela UserRoles para relacionamento muitos-para-muitos
            Create.Table("UserRoles")
                .WithColumn("UserId").AsGuid().NotNullable()
                .WithColumn("RoleId").AsGuid().NotNullable();

            // Define chave primária composta
            Create.PrimaryKey("PK_UserRoles").OnTable("UserRoles")
                .Columns("UserId", "RoleId");
        }

        public override void Down()
        {
            Delete.Table("UserRoles");
            Delete.Table("Roles");
        }
    }
}
