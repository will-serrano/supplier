namespace Supplier.Auth.Dto.Requests
{
    /// <summary>
    /// Data transfer object for registering an admin user.
    /// </summary>
    public class RegisterAdminRequestDto
    {
        /// <summary>
        /// Gets or sets the email address of the admin user.
        /// </summary>
        public string? Email { get; set; }
        /// <summary>
        /// Gets or sets the password of the admin user.
        /// </summary>
        public string? Password { get; set; }
    }
}
