using Microsoft.AspNetCore.Mvc;
using Supplier.Auth.Dto.Requests;
using Supplier.Auth.Services.Interfaces;

namespace Supplier.Auth.Controllers.Internal
{
    /// <summary>
    /// Controller for handling internal authentication operations.
    /// </summary>
    [Route("internal/auth")]
    [ApiController]
    public class InternalAuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<InternalAuthController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="InternalAuthController"/> class.
        /// </summary>
        /// <param name="authService">The authentication service.</param>
        /// <param name="logger">The logger instance.</param>
        public InternalAuthController(IAuthService authService, ILogger<InternalAuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Registers a new admin user.
        /// </summary>
        /// <param name="request">The admin registration request data transfer object.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the admin registration.</returns>
        [HttpPost("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterAdminRequestDto request)
        {
            var currentUser = HttpContext.User;
            var userName = currentUser.Identity?.Name ?? "Unknown";
            _logger.LogInformation("Admin registration attempt by user: {User}", userName);

            var response = await _authService.RegisterAdminUser(request, currentUser);

            if (response.UserId == Guid.Empty)
            {
                _logger.LogWarning("Admin registration failed for email: {Email}", request.Email);
                return BadRequest(response.Message);
            }

            _logger.LogInformation("Admin registered successfully with email: {Email}", request.Email);
            return Ok(response);
        }

        /// <summary>
        /// Retrieves all users.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing the list of all users.</returns>
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var currentUser = HttpContext.User;
            var users = await _authService.GetAllUsers(currentUser);
            return Ok(users);
        }
    }
}
