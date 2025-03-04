using Dapper;
using System.Data;

namespace Supplier.Customers.Configuration
{
    public class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
    {
        public override Guid Parse(object value)
        {
            // Se o valor for string, converte para Guid
            return value is string stringValue ? Guid.Parse(stringValue) : Guid.Empty;
        }

        public override void SetValue(IDbDataParameter parameter, Guid value)
        {
            // Armazena o Guid como string no banco
            parameter.Value = value.ToString();
            parameter.DbType = DbType.String;
        }
    }
}
