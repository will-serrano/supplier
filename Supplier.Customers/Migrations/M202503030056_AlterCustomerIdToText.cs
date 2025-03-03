using FluentMigrator;

namespace Supplier.Customers.Migrations
{
    public class M202503030056_AlterCustomerIdToText : Migration
    {
        public override void Up()
        {
            // Cria uma tabela temporária com Id como TEXT
            Create.Table("Customers_Temp")
                .WithColumn("Id").AsString(36).NotNullable().PrimaryKey() // Armazena GUID como texto
                .WithColumn("Name").AsString(255).NotNullable()
                .WithColumn("Cpf").AsString(14).NotNullable()
                .WithColumn("CreditLimit").AsDecimal().NotNullable();

            // Copia os dados da tabela original para a nova, convertendo GUID para string
            Execute.Sql(@"
                INSERT INTO Customers_Temp (Id, Name, Cpf, CreditLimit)
                SELECT hex(Id), Name, Cpf, CreditLimit FROM Customers;
            ");

            // Exclui a tabela original
            Delete.Table("Customers");

            // Renomeia a tabela temporária para "Customers"
            Rename.Table("Customers_Temp").To("Customers");
        }

        public override void Down()
        {
            // Caso precise voltar para GUID
            Create.Table("Customers_Temp")
                .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("Name").AsString(255).NotNullable()
                .WithColumn("Cpf").AsString(14).NotNullable()
                .WithColumn("CreditLimit").AsDecimal().NotNullable();

            // Copia os dados convertendo `TEXT` de volta para `BLOB` (GUID)
            Execute.Sql(@"
                INSERT INTO Customers_Temp (Id, Name, Cpf, CreditLimit)
                SELECT x' || Id, Name, Cpf, CreditLimit FROM Customers;
            ");

            // Exclui a tabela com Id como TEXT
            Delete.Table("Customers");

            // Renomeia a tabela temporária para "Customers"
            Rename.Table("Customers_Temp").To("Customers");
        }
    }
}
