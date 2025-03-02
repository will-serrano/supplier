using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Supplier.Auth.Dto.Requests;
using Supplier.Auth.Dto.Responses;
using Supplier.Auth.Repositories;
using Supplier.Auth.Services;
using Supplier.Auth.Services.Interfaces;

namespace Supplier.Auth.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            var response = await _authService.RegisterUser(request);

            if (response.UserId == Guid.Empty)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var response = await _authService.AuthenticateUser(request);

            if (response.Token == null)
                return Unauthorized(response);

            return Ok(response);
        }

        [Authorize(Roles = "admin")]
        [HttpPost("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterAdminRequestDto request)
        {
            var currentUser = HttpContext.User;
            var response = await _authService.RegisterAdminUser(request, currentUser);

            if (response.UserId == Guid.Empty)
                return BadRequest(response.Message);

            return Ok(response);
        }
    }
}
