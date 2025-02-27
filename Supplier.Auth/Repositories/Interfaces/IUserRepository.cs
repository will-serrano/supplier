using System.Collections.Generic;
using System.Threading.Tasks;
using Supplier.Auth.Models;

namespace Supplier.Auth.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<int?> CreateUser(string email, string password);
        Task<bool?> UserExists(string email);
        Task<User?> GetUserByEmail(string email);
        Task<IEnumerable<string>?> GetUserRoles(int userId);
        Task<bool> VerifyPassword(string email, string password);
    }
}
