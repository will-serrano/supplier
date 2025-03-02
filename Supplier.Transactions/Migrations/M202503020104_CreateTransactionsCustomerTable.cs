using FluentMigrator;

namespace Supplier.Transactions.Migrations
{
    [Migration(202503020104)]
    public class M202503020104_CreateTransactionsCustomerTable : Migration
    {
        public override void Up()
        {
            Create.Table("TransactionsCustomers")
                .WithColumn("Id").AsGuid().PrimaryKey()
                .WithColumn("ValorLimite").AsDecimal().NotNullable();
        }

        public override void Down()
        {
            Delete.Table("TransactionsCustomers");
        }
    }
}
