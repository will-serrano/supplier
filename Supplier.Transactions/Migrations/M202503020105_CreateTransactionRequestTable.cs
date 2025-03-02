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
                .WithColumn("TransactionId").AsGuid().NotNullable()
                .WithColumn("RequestedBy").AsString(255).Nullable()
                .WithColumn("RequestedAt").AsDateTime().NotNullable()
                .WithColumn("UpdatedBy").AsString(255).Nullable()
                .WithColumn("UpdatedAt").AsDateTime().Nullable()
                .WithColumn("Detail").AsString(1000).Nullable();

            // Adiciona chave estrangeira para o cliente, considerando que a tabela de clientes no domínio Transactions
            // foi criada com o nome "TransactionsCustomers"
            Create.ForeignKey("FK_TransactionRequests_TransactionsCustomers")
                .FromTable("TransactionRequests").ForeignColumn("CustomerId")
                .ToTable("TransactionsCustomers").PrimaryColumn("Id");
        }

        public override void Down()
        {
            Delete.ForeignKey("FK_TransactionRequests_TransactionsCustomers").OnTable("TransactionRequests");
            Delete.Table("TransactionRequests");
        }
    }
}
