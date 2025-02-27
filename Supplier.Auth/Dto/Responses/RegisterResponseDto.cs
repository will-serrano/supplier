namespace Supplier.Auth.Dto.Responses
{
    public class RegisterResponseDto
    {
        public int UserId { get; set; }
        public string Message { get; set; }

        public RegisterResponseDto(int userId, string message)
        {
            UserId = userId;
            Message = message;
        }
    }
}
