using Microsoft.AspNetCore.Identity;

namespace AMS.Authentication.Models
{
    /// <summary>
    /// Application user model extending Identity user with custom properties
    /// </summary>
    public class AppUser : IdentityUser<int>
    {
        /// <summary>
        /// User's first name
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// User's last name
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Employee ID reference
        /// </summary>
        public int? EmployeeId { get; set; }

        /// <summary>
        /// User account status
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Account creation date
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Last login timestamp
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// Account locked flag (for security purposes)
        /// </summary>
        public bool IsLocked { get; set; } = false;

        /// <summary>
        /// Last password change date
        /// </summary>
        public DateTime? LastPasswordChangeAt { get; set; }

        /// <summary>
        /// User roles navigation property
        /// </summary>
        public ICollection<AppRole> Roles { get; set; } = new List<AppRole>();

        /// <summary>
        /// Refresh tokens for this user
        /// </summary>
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

        /// <summary>
        /// Full name property
        /// </summary>
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}
