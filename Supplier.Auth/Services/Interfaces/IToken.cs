namespace Supplier.Auth.Services.Interfaces
{
    public interface IToken
    {
        string GenerateToken(Guid userId, string email, IEnumerable<string> roles);
    }
}
