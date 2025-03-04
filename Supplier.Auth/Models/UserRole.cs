namespace Supplier.Auth.Models
{
    /// <summary>
    /// Represents a user role in the authentication system.
    /// </summary>
    public class UserRole
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user role.
        /// </summary>
        public Guid? UserId { get; set; }
        /// <summary>
        /// Gets or sets the user associated with the role.
        /// </summary>
        public User? User { get; set; }
        /// <summary>
        /// Gets or sets the unique identifier for the role.
        /// </summary>
        public Guid? RoleId { get; set; }
        /// <summary>
        /// Gets or sets the role associated with the user.
        /// </summary>
        public Role? Role { get; set; }
    }
}
