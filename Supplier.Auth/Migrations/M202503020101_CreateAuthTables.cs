using FluentMigrator;

namespace Supplier.Auth.Migrations
{
    [Migration(202503020101)]
    public class M202503020101_CreateAuthTables : Migration
    {
        public override void Up()
        {
            Create.Table("Users")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("Email").AsString(255).NotNullable()
                .WithColumn("PasswordHash").AsString(255).NotNullable();
        }

        public override void Down()
        {
            Delete.Table("Users");
        }
    }
}
