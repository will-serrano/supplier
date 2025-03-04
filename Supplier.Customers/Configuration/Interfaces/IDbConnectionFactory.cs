using System.Data;

namespace Supplier.Customers.Configuration.Interfaces
{
    public interface IDbConnectionFactory
    {
        IDbConnection? CreateConnection();
    }
}
