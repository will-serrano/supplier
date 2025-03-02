using Supplier.Auth.Dto.Requests;
using Supplier.Auth.Dto.Responses;
using Supplier.Auth.Repositories.Interfaces;
using Supplier.Auth.Services.Interfaces;
using System.Security.Claims;

namespace Supplier.Auth.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IToken _jwtService;

        public AuthService(IUserRepository userRepository, IToken jwtService)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
        }

        public async Task<RegisterResponseDto> RegisterUser(RegisterRequestDto request)
        {
            var userExists = await _userRepository.UserExists(request.Email);
            if (userExists == true)
                return new RegisterResponseDto(Guid.Empty, "Usuário já cadastrado.");

            var userId = await _userRepository.CreateUser(request.Email, request.Password);
            if (userId == null || userId == Guid.Empty)
                return new RegisterResponseDto(Guid.Empty, "Erro ao criar usuário.");

            // Se o usuário não informar nenhuma role, defina como "user"
            var roles = request.Roles?.Count > 0 ? request.Roles : ["user"];

            // Atribuir roles ao usuário
            await _userRepository.AssignRolesToUser(userId.Value, roles);

            return new RegisterResponseDto(userId.Value, "Usuário cadastrado com sucesso.");
        }

        public async Task<LoginResponseDto> AuthenticateUser(LoginRequestDto request)
        {
            var user = await _userRepository.GetUserByEmail(request.Email);
            if (user == null || !await _userRepository.VerifyPassword(request.Email, request.Password))
            {
                return LoginResponseDto.WithMessage("Email ou senha inválidos.");
            }

            var roles = await _userRepository.GetUserRoles(user.Id);
            if (roles == null)
            {
                return LoginResponseDto.WithMessage("Erro ao obter roles do usuário.");
            }

            string token = _jwtService.GenerateToken(user.Id, user.Email, roles);

            return LoginResponseDto.WithToken(token);
        }

        public async Task<RegisterResponseDto> RegisterAdminUser(RegisterAdminRequestDto request, ClaimsPrincipal currentUser)
        {
            if (!currentUser.IsInRole("admin"))
            {
                return new RegisterResponseDto(Guid.Empty, "Apenas administradores podem criar outros administradores.");
            }

            var userExists = await _userRepository.UserExists(request.Email);
            if (userExists == true)
                return new RegisterResponseDto(Guid.Empty, "Usuário já cadastrado.");

            var userId = await _userRepository.CreateUser(request.Email, request.Password);
            if (userId == null || userId == Guid.Empty)
                return new RegisterResponseDto(Guid.Empty, "Erro ao criar usuário.");

            await _userRepository.AssignRolesToUser(userId.Value, new List<string> { "admin" });

            return new RegisterResponseDto(userId.Value, "Usuário administrador cadastrado com sucesso.");
        }

    }
}
