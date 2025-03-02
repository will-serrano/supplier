using Dapper;
using System.Data;

namespace Supplier.Auth.Configuration.Interfaces
{
    public interface IDapperWrapper
    {
        Task<T?> QueryFirstOrDefaultAsync<T>(IDbConnection connection, CommandDefinition command);
        Task<int> ExecuteAsync(IDbConnection connection, CommandDefinition command);
        Task<T> ExecuteScalarAsync<T>(IDbConnection connection, CommandDefinition command);
        Task<IEnumerable<T>> QueryAsync<T>(IDbConnection connection, CommandDefinition command);
    }
}
