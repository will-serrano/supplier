using FluentMigrator;

namespace Supplier.Auth.Migrations
{
    [Migration(202503020102)]
    public class M202503020102_AlterUserIdToGuid : Migration
    {
        public override void Up()
        {
            // 1. Cria a tabela temporária com o novo esquema (Id como Guid)
            Create.Table("Users_Temp")
                .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("Email").AsString(255).NotNullable()
                .WithColumn("PasswordHash").AsString(255).NotNullable();

            // 2. Copia os dados da tabela original para a nova tabela
            //    Para cada registro, gera um novo Guid (usando lower(hex(randomblob(16))) que gera uma string hexadecimal de 32 caracteres)
            Execute.Sql(@"
                INSERT INTO Users_Temp (Id, Email, PasswordHash)
                SELECT lower(hex(randomblob(16))), Email, PasswordHash FROM Users;
            ");

            // 3. Exclui a tabela original
            Delete.Table("Users");

            // 4. Renomeia a tabela temporária para o nome original
            Rename.Table("Users_Temp").To("Users");
        }

        public override void Down()
        {
            // Reverte a migração: cria uma tabela temporária com o esquema original (Id como Int32 Identity)
            Create.Table("Users_Temp")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("Email").AsString(255).NotNullable()
                .WithColumn("PasswordHash").AsString(255).NotNullable();

            // Copia os dados (a coluna Id será gerada automaticamente)
            Execute.Sql(@"
                INSERT INTO Users_Temp (Email, PasswordHash)
                SELECT Email, PasswordHash FROM Users;
            ");

            // Exclui a tabela com Id Guid
            Delete.Table("Users");

            // Renomeia a tabela temporária para o nome original
            Rename.Table("Users_Temp").To("Users");
        }
    }
}
