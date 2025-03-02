using FluentMigrator;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Supplier.Transactions.Migrations
{
    [Migration(202503020105)]
    public class M202503020105_CreateTransactionRequestTable : Migration
    {
        public override void Up()
        {
            Create.Table("TransactionRequests")
                .WithColumn("Id").AsGuid().PrimaryKey()
                .WithColumn("Amount").AsDecimal().NotNullable()
                .WithColumn("Status").AsInt32().NotNullable() // Armazena o enum como int
                .WithColumn("CustomerId").AsGuid().NotNullable()
                .WithColumn("CustomerBlocked").AsBoolean().NotNullable()
                .WithColumn("TransactionId").AsGuid().Nullable()
                .WithColumn("RequestedBy").AsString(255).Nullable()
                .WithColumn("RequestedAt").AsDateTime().NotNullable()
                .WithColumn("UpdatedBy").AsString(255).Nullable()
                .WithColumn("UpdatedAt").AsDateTime().Nullable()
                .WithColumn("Detail").AsString(1000).Nullable();
        }

        public override void Down()
        {
            Delete.Table("TransactionRequests");
        }
    }
}
