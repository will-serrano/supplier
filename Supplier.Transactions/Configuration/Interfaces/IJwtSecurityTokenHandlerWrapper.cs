using System.IdentityModel.Tokens.Jwt;

namespace Supplier.Transactions.Configuration.Interfaces
{
    public interface IJwtSecurityTokenHandlerWrapper
    {
        JwtSecurityToken ReadJwtToken(string token);
    }
}
