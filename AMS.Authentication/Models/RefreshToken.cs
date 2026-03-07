namespace AMS.Authentication.Models
{
    /// <summary>
    /// Refresh token model for maintaining long-lived sessions
    /// Following OAuth 2.0 and industry best practices
    /// </summary>
    public class RefreshToken
    {
        public int Id { get; set; }

        /// <summary>
        /// User who owns this refresh token
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Navigation property to user
        /// </summary>
        public AppUser? User { get; set; }

        /// <summary>
        /// The actual token value (encrypted/hashed)
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Expiration date of the refresh token
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// When this refresh token was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// IP address where token was created
        /// </summary>
        public string? CreatedByIp { get; set; }

        /// <summary>
        /// When this refresh token was revoked (null if active)
        /// </summary>
        public DateTime? RevokedAt { get; set; }

        /// <summary>
        /// IP address that revoked this token
        /// </summary>
        public string? RevokedByIp { get; set; }

        /// <summary>
        /// Reason for revocation
        /// </summary>
        public string? RevocationReason { get; set; }

        /// <summary>
        /// Whether this token has been revoked
        /// </summary>
        public bool IsRevoked => RevokedAt.HasValue;

        /// <summary>
        /// Whether this token is still valid
        /// </summary>
        public bool IsActive => !IsRevoked && ExpiresAt > DateTime.UtcNow;

        /// <summary>
        /// Device/user agent that created this token
        /// </summary>
        public string? UserAgent { get; set; }
    }
}
