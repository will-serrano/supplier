using Supplier.Auth.Dto.Requests;
using Supplier.Auth.Dto.Responses;

namespace Supplier.Auth.Services.Interfaces
{
    public interface IAuthService
    {
        Task<RegisterResponseDto> RegisterUser(RegisterRequestDto request);
        Task<LoginResponseDto> AuthenticateUser(LoginRequestDto request);
    }
}
