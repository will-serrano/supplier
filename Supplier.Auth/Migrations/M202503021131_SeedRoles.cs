using FluentMigrator;
using System.Collections.Generic;

namespace Supplier.Auth.Migrations
{
    [Migration(202503021131)]
    public class M202503021131_SeedRoles : Migration
    {
        // Definindo GUIDs fixos para o usuário admin e a role admin
        private readonly Guid userRoleId = new Guid("36bec067-29d1-4671-ace0-59c2a4cd86d9");

        public override void Up()
        {
            // Insere a role "admin" na tabela Roles
            Insert.IntoTable("Roles").Row(new
            {
                Id = userRoleId,
                Name = "user",
                Description = "Usuário do sistema"
            });
        }

        public override void Down()
        {
            Delete.FromTable("Roles").Row(new { Id = userRoleId });
        }
    }
}
