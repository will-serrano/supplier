namespace Supplier.Auth.Dto.Requests
{
    public class LoginResponseDto
    {
        private static LoginResponseDto? _instance;
        public string? Token { get; internal set; }
        public string? Message { get; internal set; }

        public static LoginResponseDto WithToken(string token)
        {
            _instance = new LoginResponseDto();
            _instance.Token = token;
            return _instance;
        }

        public static LoginResponseDto WithMessage(string message)
        {
            _instance = new LoginResponseDto();
            _instance.Message = message;
            return _instance;
        }
    }
}