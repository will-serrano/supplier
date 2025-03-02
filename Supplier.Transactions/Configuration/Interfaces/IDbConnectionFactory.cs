using System.Data;

namespace Supplier.Transactions.Configuration.Interfaces
{
    public interface IDbConnectionFactory
    {
        IDbConnection? CreateConnection();
    }
}
