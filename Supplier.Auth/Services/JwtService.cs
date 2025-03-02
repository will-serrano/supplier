using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Supplier.Auth.Configuration;
using Supplier.Auth.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Supplier.Auth.Services
{
    public class JwtService : IToken
    {
        private readonly JwtSettings _jwtSettings;

        public JwtService(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
        }

        public string GenerateToken(Guid userId, string email, IEnumerable<string> roles)
        {
            // Validação da chave JWT
            if (string.IsNullOrWhiteSpace(_jwtSettings.Secret) || _jwtSettings.Secret.Length < 32)
            {
                throw new ArgumentException("Chave JWT inválida ou muito curta.", nameof(_jwtSettings.Secret));
            }

            // Validação do tempo de expiração
            if (_jwtSettings.ExpirationMinutes <= 0)
            {
                throw new ArgumentException("O tempo de expiração do token deve ser maior que zero.", nameof(_jwtSettings.ExpirationMinutes));
            }

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, userId.ToString()), // Identificador único
                new(ClaimTypes.NameIdentifier, userId.ToString()),  // ID do usuário
                new(ClaimTypes.Email, email), // Email
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Token único
            };

            // Garantindo que roles não seja nulo
            roles ??= Enumerable.Empty<string>();
            foreach (var role in roles)
            {
                claims.Add(new(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
