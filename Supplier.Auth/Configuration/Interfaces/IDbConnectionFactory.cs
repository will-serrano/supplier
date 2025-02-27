using System.Data;

namespace Supplier.Auth.Configuration.Interfaces
{
    public interface IDbConnectionFactory
    {
        IDbConnection? CreateConnection();
    }
}
