using FluentMigrator;
using System.Collections.Generic;

namespace Supplier.Auth.Migrations
{
    [Migration(202503020107)]
    public class M202503020107_SeedAdminUser : Migration
    {
        // Definindo GUIDs fixos para o usuário admin e a role admin
        private readonly Guid adminUserId = new Guid("f438e0ae-6d58-4a59-918c-08324e342c0b");
        private readonly Guid adminRoleId = new Guid("bc747097-7135-468a-95fc-84dcb4897cc1");

        public override void Up()   
        {
            // Insere a role "admin" na tabela Roles
            Insert.IntoTable("Roles").Row(new
            {
                Id = adminRoleId,
                Name = "admin",
                Description = "Administrador do sistema"
            });

            // Insere o usuário admin na tabela Users
            // Senha "SenhaForte@123"
            Insert.IntoTable("Users").Row(new
            {
                Id = adminUserId,
                Email = "admin@supplier.com",
                PasswordHash = "d4f6a704e14baa175f4d41d58fe1b055f8153fa2a108faa3a5021c3895b2b414"
            });

            // Associa o usuário admin à role admin na tabela UserRoles
            Insert.IntoTable("UserRoles").Row(new
            {
                UserId = adminUserId,
                RoleId = adminRoleId
            });
        }

        public override void Down()
        {
            // Remove a associação, o usuário e a role (na ordem inversa)
            Delete.FromTable("UserRoles").Row(new { UserId = adminUserId, RoleId = adminRoleId });
            Delete.FromTable("Users").Row(new { Id = adminUserId });
            Delete.FromTable("Roles").Row(new { Id = adminRoleId });
        }
    }
}
