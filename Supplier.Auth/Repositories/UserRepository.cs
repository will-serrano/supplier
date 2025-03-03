using Dapper;
using Microsoft.AspNetCore.Identity;
using Supplier.Auth.Configuration.Interfaces;
using Supplier.Auth.Models;
using Supplier.Auth.Repositories.Interfaces;
using System.Data;

namespace Supplier.Auth.Repositories
{
    /// <summary>
    /// Repository for managing user data.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        private readonly IPasswordHasher<IdentityUser> _passwordHasher;
        private readonly IDapperWrapper _dapperWrapper;
        private readonly ILogger<UserRepository> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserRepository"/> class.
        /// </summary>
        /// <param name="dbConnectionFactory">The database connection factory.</param>
        /// <param name="passwordHasher">The password hasher.</param>
        /// <param name="dapperWrapper">The Dapper wrapper.</param>
        /// <param name="logger">The logger.</param>
        public UserRepository(
            IDbConnectionFactory dbConnectionFactory,
            IPasswordHasher<IdentityUser> passwordHasher,
            IDapperWrapper dapperWrapper,
            ILogger<UserRepository> logger)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _passwordHasher = passwordHasher;
            _dapperWrapper = dapperWrapper;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new user with the specified email and password.
        /// </summary>
        /// <param name="email">The email of the user.</param>
        /// <param name="password">The password of the user.</param>
        /// <returns>The ID of the created user, or null if creation failed.</returns>
        public async Task<Guid?> CreateUser(string email, string password)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            if (connection == null)
            {
                _logger.LogError("Failed to create a connection to the database.");
                return null;
            }

            // Check if the user already exists
            if (await UserExists(email) == true)
            {
                _logger.LogInformation("User with email {Email} already exists.", email);
                return null;
            }

            string hashedPassword = _passwordHasher.HashPassword(new IdentityUser { Email = email }, password);
            string query = @"
                                    INSERT INTO Users (Id, Email, PasswordHash) 
                                    VALUES (@Id, @Email, @PasswordHash); 
                                    SELECT @Id;";

            var id = Guid.NewGuid();

            try
            {
                return await _dapperWrapper.ExecuteScalarAsync<Guid>(connection, new CommandDefinition(query, new
                {
                    Id = id,
                    Email = email,
                    PasswordHash = hashedPassword
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing CreateUser query.");
                return null;
            }
        }

        /// <summary>
        /// Checks if a user with the specified email exists.
        /// </summary>
        /// <param name="email">The email of the user.</param>
        /// <returns>True if the user exists, otherwise false.</returns>
        public async Task<bool?> UserExists(string email)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            if (connection == null)
            {
                _logger.LogError("Failed to create a connection to the database.");
                return null;
            }

            var exists = await _dapperWrapper.ExecuteScalarAsync<int>(connection, new CommandDefinition(
                "SELECT COUNT(1) FROM Users WHERE Email = @Email",
                new { Email = email }
            ));
            return exists > 0;
        }

        /// <summary>
        /// Gets a user by their email.
        /// </summary>
        /// <param name="email">The email of the user.</param>
        /// <returns>The user with the specified email, or null if not found.</returns>
        public async Task<User?> GetUserByEmail(string email)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            if (connection == null)
            {
                _logger.LogError("Failed to create a connection to the database.");
                return null;
            }

            return await _dapperWrapper.QueryFirstOrDefaultAsync<User>(connection, new CommandDefinition(
                "SELECT * FROM Users WHERE Email = @Email",
                new { Email = email }
            ));
        }

        /// <summary>
        /// Gets the roles of a user by their ID.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>A list of role names assigned to the user.</returns>
        public async Task<IEnumerable<string>?> GetUserRoles(Guid userId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            if (connection == null)
            {
                _logger.LogError("Failed to create a connection to the database.");
                return null;
            }

            return await _dapperWrapper.QueryAsync<string>(connection, new CommandDefinition(
                @"SELECT r.Name 
                    FROM Roles r 
                    INNER JOIN UserRoles ur ON r.Id = ur.RoleId 
                    WHERE ur.UserId = @UserId",
                new { UserId = userId }
            ));
        }

        /// <summary>
        /// Verifies the password of a user.
        /// </summary>
        /// <param name="email">The email of the user.</param>
        /// <param name="password">The password to verify.</param>
        /// <returns>True if the password is correct, otherwise false.</returns>
        public async Task<bool> VerifyPassword(string email, string password)
        {
            var user = await GetUserByEmail(email);
            if (user == null)
                return false;

            var verificationResult = _passwordHasher.VerifyHashedPassword(new IdentityUser { Email = email }, user.PasswordHash, password);
            return verificationResult == PasswordVerificationResult.Success;
        }

        /// <summary>
        /// Assigns roles to a user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="roles">The list of roles to assign.</param>
        public async Task AssignRolesToUser(Guid userId, List<string> roles)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            if (connection == null)
            {
                throw new InvalidOperationException("Database connection is not available.");
            }

            var roleNames = roles.Select(role => role.ToUpper()).ToList();
            var existingRoles = await _dapperWrapper.QueryAsync<Role>(connection, new CommandDefinition("SELECT * FROM Roles WHERE upper(Name) IN @Names", new { Names = roleNames }));

            var rolesToInsert = roles.Except(existingRoles.Select(r => r.Name), StringComparer.OrdinalIgnoreCase).ToList();
            foreach (var role in rolesToInsert)
            {
                await _dapperWrapper.ExecuteAsync(connection, new CommandDefinition("INSERT INTO Roles (Name) VALUES (@Name)", new { Name = role }));
            }

            existingRoles = await _dapperWrapper.QueryAsync<Role>(connection, new CommandDefinition("SELECT * FROM Roles WHERE upper(Name) IN @Names", new { Names = roleNames }));

            foreach (var role in existingRoles)
            {
                await _dapperWrapper.ExecuteAsync(connection, new CommandDefinition("INSERT INTO UserRoles (UserId, RoleId) VALUES (@UserId, @RoleId)", new { UserId = userId, RoleId = role.Id }));
            }
        }
    }
}
