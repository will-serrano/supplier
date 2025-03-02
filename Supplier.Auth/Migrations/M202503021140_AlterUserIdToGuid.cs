using FluentMigrator;

namespace Supplier.Auth.Migrations
{
    [Migration(202503020102)]
    public class M202503020102_AlterUserIdToGuid : Migration
    {
        public override void Up()
        {
            // Criar uma nova coluna temporária com GUID
            Alter.Table("Users")
                .AddColumn("Id_New").AsGuid().NotNullable().WithDefault(SystemMethods.NewGuid);

            // Copiar os dados da coluna antiga para a nova (não há conversão direta de Int32 para Guid, será necessário um ajuste manual se aplicável)

            // Remover a chave primária da coluna antiga
            Delete.PrimaryKey("Users").FromTable("Users");

            // Remover a coluna antiga
            Delete.Column("Id").FromTable("Users");

            // Renomear a nova coluna para "Id"
            Rename.Column("Id_New").OnTable("Users").To("Id");

            // Definir a nova coluna como chave primária
            Create.PrimaryKey("PK_Users").OnTable("Users").Column("Id");
        }

        public override void Down()
        {
            // Criar uma nova coluna temporária com Int32 (caso precise reverter)
            Alter.Table("Users")
                .AddColumn("Id_Old").AsInt32().NotNullable().Identity();

            // Remover a chave primária da coluna GUID
            Delete.PrimaryKey("PK_Users").FromTable("Users");

            // Remover a coluna GUID
            Delete.Column("Id").FromTable("Users");

            // Renomear a coluna de volta para "Id"
            Rename.Column("Id_Old").OnTable("Users").To("Id");

            // Definir novamente como chave primária
            Create.PrimaryKey("PK_Users").OnTable("Users").Column("Id");
        }
    }
}
