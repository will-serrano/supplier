using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Supplier.Auth.Dto.Requests;
using Supplier.Auth.Services.Interfaces;

namespace Supplier.Auth.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class.
        /// </summary>
        /// <param name="authService">The authentication service.</param>
        /// <param name="logger">The logger instance.</param>
        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="request">The registration request data transfer object.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the registration.</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            _logger.LogInformation("Register attempt for email: {Email}", request.Email);

            var response = await _authService.RegisterUser(request);

            if (response.UserId == Guid.Empty)
            {
                _logger.LogWarning("Registration failed for email: {Email}", request.Email);
                return BadRequest(response);
            }

            _logger.LogInformation("User registered successfully with email: {Email}", request.Email);
            return Ok(response);
        }

        /// <summary>
        /// Authenticates a user and generates a token.
        /// </summary>
        /// <param name="request">The login request data transfer object.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the authentication.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            _logger.LogInformation("Login attempt for email: {Email}", request.Email);

            var response = await _authService.AuthenticateUser(request);
            
            if (response.Token == null)
            {
                _logger.LogWarning("Login failed for email: {Email}", request.Email);
                return Unauthorized(response);
            }

            _logger.LogInformation("User logged in successfully with email: {Email}", request.Email);
            return Ok(response);
        }
    }
}
