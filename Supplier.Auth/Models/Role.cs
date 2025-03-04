namespace Supplier.Auth.Models
{
    /// <summary>
    /// Represents a role in the authentication system.
    /// </summary>
    public class Role
    {
        /// <summary>
        /// Gets or sets the unique identifier for the role.
        /// </summary>
        public Guid? Id { get; set; }
        /// <summary>
        /// Gets or sets the name of the role.
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// Gets or sets the description of the role.
        /// </summary>
        public string? Description { get; set; }
    }
}
