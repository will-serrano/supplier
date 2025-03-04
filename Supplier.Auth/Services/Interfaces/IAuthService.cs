using Supplier.Auth.Dto.Requests;
using Supplier.Auth.Dto.Responses;
using Supplier.Auth.Models;
using System.Security.Claims;

namespace Supplier.Auth.Services.Interfaces
{
    public interface IAuthService
    {
        Task<RegisterResponseDto> RegisterUser(RegisterRequestDto request);
        Task<LoginResponseDto> AuthenticateUser(LoginRequestDto request);
        Task<RegisterResponseDto> RegisterAdminUser(RegisterAdminRequestDto request, ClaimsPrincipal currentUser);
        Task<IEnumerable<UserResponseDto>> GetAllUsers(ClaimsPrincipal currentUser);
    }
}
