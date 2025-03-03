using FluentMigrator;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Supplier.Transactions.Migrations
{
    /// <summary>
    /// Migration class to create the TransactionRequests table.
    /// </summary>
    [Migration(202503031707)]
    public class M202503031707_CreateTransactionRequestTable : Migration
    {
        /// <summary>
        /// Defines the operations to be performed when applying the migration.
        /// </summary>
        public override void Up()
        {
            Create.Table("TransactionRequests")
                .WithColumn("Id").AsString(36).NotNullable().PrimaryKey() // Armazena GUID como texto
                .WithColumn("Amount").AsDecimal().NotNullable()
                .WithColumn("Status").AsInt32().NotNullable() // Enum como int
                .WithColumn("CustomerId").AsString(36).NotNullable() // Armazena GUID como texto
                .WithColumn("CustomerBlocked").AsBoolean().NotNullable()
                .WithColumn("TransactionId").AsString(36).Nullable() // Armazena GUID como texto
                .WithColumn("RequestedBy").AsString(255).Nullable()
                .WithColumn("RequestedAt").AsDateTime().NotNullable()
                .WithColumn("UpdatedBy").AsString(255).Nullable()
                .WithColumn("UpdatedAt").AsDateTime().Nullable()
                .WithColumn("Detail").AsString(1000).Nullable();
        }

        /// <summary>
        /// Defines the operations to be performed when rolling back the migration.
        /// </summary>
        public override void Down()
        {
            Delete.Table("TransactionRequests");
        }
    }
}
