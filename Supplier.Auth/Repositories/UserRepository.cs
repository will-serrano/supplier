using Dapper;
using Microsoft.AspNetCore.Identity;
using Serilog;
using Supplier.Auth.Configuration.Interfaces;
using Supplier.Auth.Models;
using Supplier.Auth.Repositories.Interfaces;

namespace Supplier.Auth.Repositories
{
    public class UserRepository: IUserRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        private readonly IPasswordHasher<IdentityUser> _passwordHasher;

        public UserRepository(
            IDbConnectionFactory dbConnectionFactory,
            IPasswordHasher<IdentityUser> passwordHasher
        )
        {
            _dbConnectionFactory = dbConnectionFactory;
            _passwordHasher = passwordHasher;
        }

        public async Task<int?> CreateUser(string email, string password)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            string hashedPassword = _passwordHasher.HashPassword(new IdentityUser { Email = email }, password);


            string query = @"
                INSERT INTO Users (Email, PasswordHash) 
                VALUES (@Email, @PasswordHash); 
                SELECT last_insert_rowid();";

            if (connection == null)
            {
                Log.Error("Falha ao criar conexão com o banco de dados");
                return null;
            }

            return await connection.ExecuteScalarAsync<int>(query, new
            {
                Email = email,
                PasswordHash = hashedPassword
            });
        }

        public async Task<bool?> UserExists(string email)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            if (connection == null)
            {
                Log.Error("Falha ao criar conexão com o banco de dados");
                return null;
            }
            var exists = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM Users WHERE Email = @Email",
                new { Email = email }
            );
            return exists > 0;
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            if (connection == null)
            {
                Log.Error("Falha ao criar conexão com o banco de dados");
                return null;
            }
            return await connection.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM Users WHERE Email = @Email",
                new { Email = email }
            );
        }

        public async Task<IEnumerable<string>?> GetUserRoles(int userId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            if (connection == null)
            {
                Log.Error("Falha ao criar conexão com o banco de dados");
                return null;
            }
            return await connection.QueryAsync<string>(
                @"SELECT r.Name 
              FROM Roles r 
              INNER JOIN UserRoles ur ON r.Id = ur.RoleId 
              WHERE ur.UserId = @UserId",
                new { UserId = userId }
            );
        }

        public async Task<bool> VerifyPassword(string email, string password)
        {
            var user = await GetUserByEmail(email);
            if (user == null)
                return false;

            var verificationResult = _passwordHasher.VerifyHashedPassword(new IdentityUser { Email = email }, user.PasswordHash, password);
            return verificationResult == PasswordVerificationResult.Success;
        }
    }
}
