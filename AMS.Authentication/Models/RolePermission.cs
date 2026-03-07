namespace AMS.Authentication.Models
{
    /// <summary>
    /// Role-Permission mapping for fine-grained access control
    /// Follows principle of least privilege
    /// </summary>
    public class RolePermission
    {
        public int Id { get; set; }

        /// <summary>
        /// Role that has this permission
        /// </summary>
        public int RoleId { get; set; }

        /// <summary>
        /// Navigation property to role
        /// </summary>
        public AppRole? Role { get; set; }

        /// <summary>
        /// Permission identifier (e.g., "attendance:create", "attendance:read", etc.)
        /// Format: "resource:action"
        /// </summary>
        public string PermissionName { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable permission description
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Date when this permission was assigned
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Whether this permission is active
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
