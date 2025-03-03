namespace Supplier.Auth.Dto.Responses
{
    /// <summary>
    /// Data transfer object for user response.
    /// </summary>
    public class UserResponseDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user.
        /// </summary>
        public Guid? Id { get; set; }
        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        public required string Email { get; set; }
    }
}