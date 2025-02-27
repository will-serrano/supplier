namespace Supplier.Auth.Services.Interfaces
{
    public interface IToken
    {
        string GenerateToken(int userId, string email, IEnumerable<string> roles);
    }
}
