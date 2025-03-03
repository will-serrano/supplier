using FluentMigrator;

namespace Supplier.Auth.Migrations
{
    /// <summary>
    /// Migration class to seed initial roles.
    /// </summary>
    [Migration(202503031707)]
    public class M202503031707_SeedRoles : Migration
    {
        private readonly string adminRoleId = "bc747097-7135-468a-95fc-84dcb4897cc1";
        private readonly string userRoleId = "36bec067-29d1-4671-ace0-59c2a4cd86d9";

        /// <summary>
        /// Defines the operations to be performed when applying the migration.
        /// </summary>
        public override void Up()
        {
            Insert.IntoTable("Roles").Row(new
            {
                Id = adminRoleId,
                Name = "admin",
                Description = "Administrador do sistema"
            });

            Insert.IntoTable("Roles").Row(new
            {
                Id = userRoleId,
                Name = "user",
                Description = "Usuário do sistema"
            });
        }

        /// <summary>
        /// Defines the operations to be performed when rolling back the migration.
        /// </summary>
        public override void Down()
        {
            Delete.FromTable("Roles").Row(new { Id = adminRoleId });
            Delete.FromTable("Roles").Row(new { Id = userRoleId });
        }
    }
}
