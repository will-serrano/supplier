using Supplier.Transactions.Configuration.Interfaces;
using System.IdentityModel.Tokens.Jwt;

namespace Supplier.Transactions.Configuration
{
    public class JwtSecurityTokenHandlerWrapper : IJwtSecurityTokenHandlerWrapper
    {
        private readonly JwtSecurityTokenHandler _tokenHandler = new JwtSecurityTokenHandler();

        public JwtSecurityToken ReadJwtToken(string token)
        {
            return _tokenHandler.ReadJwtToken(token);
        }
    }
}
