namespace Supplier.Auth.Models
{
    /// <summary>
    /// Represents a user in the authentication system.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user.
        /// </summary>
        public Guid? Id { get; set; }
        /// <summary>
        /// Gets or sets the email address of the user. 
        /// </summary>
        public required string Email { get; set; }
        /// <summary>
        /// Gets or sets the password hash of the user.
        /// </summary>
        public required string PasswordHash { get; set; }
    }
}
