using Microsoft.AspNetCore.Identity;

namespace AMS.Authentication.Models
{
    /// <summary>
    /// Application role model extending Identity role with custom properties
    /// </summary>
    public class AppRole : IdentityRole<int>
    {
        /// <summary>
        /// Role description
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Role priority for permission resolution (higher = more privileged)
        /// </summary>
        public int Priority { get; set; } = 0;

        /// <summary>
        /// Whether this is a system role that should not be deleted
        /// </summary>
        public bool IsSystemRole { get; set; } = false;

        /// <summary>
        /// Permissions assigned to this role
        /// </summary>
        public ICollection<RolePermission> Permissions { get; set; } = new List<RolePermission>();

        /// <summary>
        /// Users assigned to this role
        /// </summary>
        public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
    }
}
