using FluentMigrator;

namespace Supplier.Transactions.Migrations
{
    public class M202503030058_AlterTransactionRequestIdToText : Migration
    {
        public override void Up()
        {
            // Cria uma tabela temporária com os campos de GUID convertidos para TEXT
            Create.Table("TransactionRequests_Temp")
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

            // Copia os dados da tabela original para a nova, convertendo GUIDs para string
            Execute.Sql(@"
                INSERT INTO TransactionRequests_Temp (Id, Amount, Status, CustomerId, CustomerBlocked, TransactionId, RequestedBy, RequestedAt, UpdatedBy, UpdatedAt, Detail)
                SELECT hex(Id), Amount, Status, hex(CustomerId), CustomerBlocked, CASE WHEN TransactionId IS NOT NULL THEN hex(TransactionId) ELSE NULL END, RequestedBy, RequestedAt, UpdatedBy, UpdatedAt, Detail
                FROM TransactionRequests;
            ");

            // Exclui a tabela original
            Delete.Table("TransactionRequests");

            // Renomeia a tabela temporária para "TransactionRequests"
            Rename.Table("TransactionRequests_Temp").To("TransactionRequests");
        }

        public override void Down()
        {
            // Caso precise voltar para GUID
            Create.Table("TransactionRequests_Temp")
                .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("Amount").AsDecimal().NotNullable()
                .WithColumn("Status").AsInt32().NotNullable()
                .WithColumn("CustomerId").AsGuid().NotNullable()
                .WithColumn("CustomerBlocked").AsBoolean().NotNullable()
                .WithColumn("TransactionId").AsGuid().Nullable()
                .WithColumn("RequestedBy").AsString(255).Nullable()
                .WithColumn("RequestedAt").AsDateTime().NotNullable()
                .WithColumn("UpdatedBy").AsString(255).Nullable()
                .WithColumn("UpdatedAt").AsDateTime().Nullable()
                .WithColumn("Detail").AsString(1000).Nullable();

            // Copia os dados convertendo `TEXT` de volta para `BLOB` (GUID)
            Execute.Sql(@"
                INSERT INTO TransactionRequests_Temp (Id, Amount, Status, CustomerId, CustomerBlocked, TransactionId, RequestedBy, RequestedAt, UpdatedBy, UpdatedAt, Detail)
                SELECT x' || Id, Amount, Status, x' || CustomerId, CustomerBlocked, CASE WHEN TransactionId IS NOT NULL THEN x' || TransactionId ELSE NULL END, RequestedBy, RequestedAt, UpdatedBy, UpdatedAt, Detail
                FROM TransactionRequests;
            ");

            // Exclui a tabela com Id como TEXT
            Delete.Table("TransactionRequests");

            // Renomeia a tabela temporária para "TransactionRequests"
            Rename.Table("TransactionRequests_Temp").To("TransactionRequests");
        }
    }
}
