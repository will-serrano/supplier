using Dapper;
using Supplier.Transactions.Repositories.Interfaces;
using System.Data;

namespace Supplier.Transactions.Repositories
{
    /// <summary>
    /// Provides a wrapper for Dapper methods to interact with the database.
    /// </summary>
    public class DapperWrapper : IDapperWrapper
    {
        /// <summary>
        /// Executes a query and returns the first result or a default value.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="connection">The database connection.</param>
        /// <param name="command">The command definition.</param>
        /// <returns>The first result or a default value.</returns>
        public Task<T?> QueryFirstOrDefaultAsync<T>(IDbConnection connection, CommandDefinition command)
        {
            return connection.QueryFirstOrDefaultAsync<T?>(command);
        }

        /// <summary>
        /// Executes a command asynchronously.
        /// </summary>
        /// <param name="connection">The database connection.</param>
        /// <param name="command">The command definition.</param>
        /// <returns>The number of rows affected.</returns>
        public Task<int> ExecuteAsync(IDbConnection connection, CommandDefinition command)
        {
            return connection.ExecuteAsync(command);
        }

        /// <summary>
        /// Executes a scalar command asynchronously and returns the result.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="connection">The database connection.</param>
        /// <param name="command">The command definition.</param>
        /// <returns>The result of the scalar command.</returns>
        public async Task<T> ExecuteScalarAsync<T>(IDbConnection connection, CommandDefinition command)
        {
            var result = await connection.ExecuteScalarAsync<T>(command);
            return result!;
        }

        /// <summary>
        /// Executes a query asynchronously and returns the result.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="connection">The database connection.</param>
        /// <param name="command">The command definition.</param>
        /// <returns>The result of the query.</returns>
        public async Task<IEnumerable<T>> QueryAsync<T>(IDbConnection connection, CommandDefinition command)
        {
            var result = await connection.QueryAsync<T>(command);
            return result;
        }

        /// <summary>
        /// Executes a query and returns a single result or a default value.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="connection">The database connection.</param>
        /// <param name="command">The command definition.</param>
        /// <returns>The single result or a default value.</returns>
        public Task<T?> QuerySingleOrDefaultAsync<T>(IDbConnection connection, CommandDefinition command)
        {
            return connection.QuerySingleOrDefaultAsync<T?>(command);
        }
    }
}
