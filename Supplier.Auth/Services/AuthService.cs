using Supplier.Auth.Dto.Requests;
using Supplier.Auth.Dto.Responses;
using Supplier.Auth.Repositories.Interfaces;
using Supplier.Auth.Services.Interfaces;
using System.Security.Claims;

namespace Supplier.Auth.Services
{
    /// <summary>
    /// Service for handling authentication and user registration.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IToken _jwtService;
        private readonly ILogger<AuthService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthService"/> class.
        /// </summary>
        /// <param name="userRepository">The user repository.</param>
        /// <param name="jwtService">The JWT service.</param>
        /// <param name="logger">The logger.</param>
        public AuthService(IUserRepository userRepository, IToken jwtService, ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _logger = logger;
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="request">The registration request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the registration response.</returns>
        public async Task<RegisterResponseDto> RegisterUser(RegisterRequestDto request)
        {
            var userExists = await _userRepository.UserExists(request.Email);
            if (userExists == true)
                return new RegisterResponseDto(Guid.Empty, "User already registered.");

            var userId = await _userRepository.CreateUser(request.Email, request.Password);
            if (userId == null || userId == Guid.Empty)
                return new RegisterResponseDto(Guid.Empty, "Error creating user.");

            // Se o usuário não informar nenhuma role, defina como "user"
            var roles = request.Roles?.Count > 0 ? request.Roles : new List<string> { "user" };

            // Atribuir roles ao usuário
            await _userRepository.AssignRolesToUser(userId.Value, roles);

            return new RegisterResponseDto(userId.Value, "User successfully registered.");
        }

        /// <summary>
        /// Authenticates a user.
        /// </summary>
        /// <param name="request">The login request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the login response.</returns>
        public async Task<LoginResponseDto> AuthenticateUser(LoginRequestDto request)
        {
            var user = await _userRepository.GetUserByEmail(request.Email);
            if (user == null || !await _userRepository.VerifyPassword(request.Email, request.Password))
            {
                return LoginResponseDto.WithMessage("Invalid email or password.");
            }

            var roles = await _userRepository.GetUserRoles(user.Id);
            if (roles == null)
            {
                return LoginResponseDto.WithMessage("Error retrieving user roles.");
            }

            string token = _jwtService.GenerateToken(user.Id, user.Email, roles);

            return LoginResponseDto.WithToken(token);
        }

        /// <summary>
        /// Registers a new admin user.
        /// </summary>
        /// <param name="request">The registration request.</param>
        /// <param name="currentUser">The current user.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the registration response.</returns>
        public async Task<RegisterResponseDto> RegisterAdminUser(RegisterAdminRequestDto request, ClaimsPrincipal currentUser)
        {
            var adminIdClaim = currentUser.FindFirst(ClaimTypes.NameIdentifier);
            if (adminIdClaim == null)
            {
                _logger.LogWarning("Current user is not authenticated.");
                return new RegisterResponseDto(Guid.Empty, "Current user is not authenticated.");
            }

            var adminId = Guid.Parse(adminIdClaim.Value);
            var isAdmin = await _userRepository.IsUserInRole(adminId, "admin");

            if (!isAdmin)
            {
                _logger.LogWarning("Only administrators can create other administrators.");
                return new RegisterResponseDto(Guid.Empty, "Only administrators can create other administrators.");
            }

            var userExists = await _userRepository.UserExists(request.Email);
            if (userExists == true)
                return new RegisterResponseDto(Guid.Empty, "User already registered.");

            var userId = await _userRepository.CreateUser(request.Email, request.Password);
            if (userId == null || userId == Guid.Empty)
                return new RegisterResponseDto(Guid.Empty, "Error creating user.");

            await _userRepository.AssignRolesToUser(userId.Value, new List<string> { "admin" });

            return new RegisterResponseDto(userId.Value, "Admin user successfully registered.");
        }

        /// <summary>
        /// Retrieves all users.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of user responses.</returns>
        public async Task<IEnumerable<UserResponseDto>> GetAllUsers(ClaimsPrincipal currentUser)
        {
            var adminIdClaim = currentUser.FindFirst(ClaimTypes.NameIdentifier);
            if (adminIdClaim == null)
            {
                _logger.LogWarning("Current user is not authenticated.");
                return Enumerable.Empty<UserResponseDto>();
            }

            var adminId = Guid.Parse(adminIdClaim.Value);
            var isAdmin = await _userRepository.IsUserInRole(adminId, "admin");

            if (!isAdmin)
            {
                _logger.LogWarning("Only administrators can retrieve all users.");
                return Enumerable.Empty<UserResponseDto>();
            }

            return await _userRepository.GetAllUsers();
        }
    }
}
