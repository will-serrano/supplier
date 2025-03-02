namespace Supplier.Auth.Dto.Responses
{
    public class RegisterResponseDto
    {
        public Guid UserId { get; set; }
        public string Message { get; set; }

        public RegisterResponseDto(Guid userId, string message)
        {
            UserId = userId;
            Message = message;
        }
    }
}
