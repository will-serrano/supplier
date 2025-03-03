namespace Supplier.Auth.Dto.Responses
{
    public class UserResponseDto
    {
        public Guid Id { get; set; }
        public required string Email { get; set; }
    }
}