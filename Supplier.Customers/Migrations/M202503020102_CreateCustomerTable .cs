using FluentMigrator;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Supplier.Customers.Migrations
{
    [Migration(202503020102)]
    public class M202503020102_CreateCustomerTable : Migration
    {
        public override void Up()
        {
            Create.Table("Customers")
                .WithColumn("Id").AsGuid().PrimaryKey()
                .WithColumn("Name").AsString(255).NotNullable()
                .WithColumn("Cpf").AsString(14).NotNullable()
                .WithColumn("CreditLimit").AsDecimal().NotNullable();
        }

        public override void Down()
        {
            Delete.Table("Customers");
        }
    }
}
