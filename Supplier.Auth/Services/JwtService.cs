using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Supplier.Auth.Configuration;
using Supplier.Auth.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Supplier.Auth.Services
{
    /// <summary>
    /// Service for generating JWT tokens.
    /// </summary>
    public class JwtService : IToken
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<JwtService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtService"/> class.
        /// </summary>
        /// <param name="jwtSettings">The JWT settings.</param>
        /// <param name="logger">The logger instance.</param>
        public JwtService(IOptions<JwtSettings> jwtSettings, ILogger<JwtService> logger)
        {
            _jwtSettings = jwtSettings.Value;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generates a JWT token.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="email">The user's email.</param>
        /// <param name="roles">The user's roles.</param>
        /// <returns>The generated JWT token.</returns>
        /// <exception cref="ArgumentException">Thrown when the JWT secret is invalid or the expiration time is not greater than zero.</exception>
        public string GenerateToken(Guid userId, string email, IEnumerable<string> roles)
        {
            _logger.LogInformation("Generating JWT token for user {UserId} with email {Email}", userId, email);

            // JWT key validation
            if (string.IsNullOrWhiteSpace(_jwtSettings.Secret) || _jwtSettings.Secret.Length < 32)
            {
                _logger.LogError("Invalid or too short JWT secret.");
                throw new ArgumentException("Invalid or too short JWT secret.", nameof(_jwtSettings.Secret));
            }

            // Expiration time validation
            if (_jwtSettings.ExpirationMinutes <= 0)
            {
                _logger.LogError("Token expiration time must be greater than zero.");
                throw new ArgumentException("Token expiration time must be greater than zero.", nameof(_jwtSettings.ExpirationMinutes));
            }

            var claims = new List<Claim>
                {
                    new(JwtRegisteredClaimNames.Sub, userId.ToString()), // Unique identifier
                    new(ClaimTypes.NameIdentifier, userId.ToString()),  // User ID
                    new(ClaimTypes.Email, email), // Email
                    new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Unique token
                };

            // Ensuring roles is not null
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

            _logger.LogInformation("JWT token generated successfully for user {UserId}", userId);

            return tokenHandler.WriteToken(token);
        }
    }
}
