namespace AMS.Authentication.Configuration
{
    /// <summary>
    /// JWT configuration settings
    /// </summary>
    public class JwtSettings
    {
        /// <summary>
        /// Secret key for signing tokens (minimum 32 characters for HS256)
        /// IMPORTANT: In production, use environment variables or secure key management
        /// </summary>
        public string SecretKey { get; set; } = string.Empty;

        /// <summary>
        /// Issuer of the token
        /// </summary>
        public string Issuer { get; set; } = "AMS";

        /// <summary>
        /// Audience/consumer of the token
        /// </summary>
        public string Audience { get; set; } = "AMSUsers";

        /// <summary>
        /// Access token expiration time in minutes (default: 15 minutes)
        /// </summary>
        public int AccessTokenExpirationMinutes { get; set; } = 15;

        /// <summary>
        /// Refresh token expiration time in days (default: 7 days)
        /// </summary>
        public int RefreshTokenExpirationDays { get; set; } = 7;

        /// <summary>
        /// Enable HTTPS requirement for token issuance
        /// </summary>
        public bool RequireHttpsMetadata { get; set; } = true;

        /// <summary>
        /// Validate token issuer
        /// </summary>
        public bool ValidateIssuer { get; set; } = true;

        /// <summary>
        /// Validate token audience
        /// </summary>
        public bool ValidateAudience { get; set; } = true;

        /// <summary>
        /// Validate token signature
        /// </summary>
        public bool ValidateIssuerSigningKey { get; set; } = true;

        /// <summary>
        /// Validate token lifetime
        /// </summary>
        public bool ValidateLifetime { get; set; } = true;

        /// <summary>
        /// Clock skew for token expiration (in seconds) - graceful expiration margin
        /// </summary>
        public int ClockSkewSeconds { get; set; } = 60;
    }

    /// <summary>
    /// Password policy configuration
    /// </summary>
    public class PasswordPolicy
    {
        /// <summary>
        /// Minimum password length
        /// </summary>
        public int MinimumLength { get; set; } = 8;

        /// <summary>
        /// Require uppercase characters
        /// </summary>
        public bool RequireUppercase { get; set; } = true;

        /// <summary>
        /// Require lowercase characters
        /// </summary>
        public bool RequireLowercase { get; set; } = true;

        /// <summary>
        /// Require digits/numbers
        /// </summary>
        public bool RequireDigits { get; set; } = true;

        /// <summary>
        /// Require non-alphanumeric characters (e.g., !@#$%^&*)
        /// </summary>
        public bool RequireNonAlphanumericCharacters { get; set; } = true;

        /// <summary>
        /// Maximum password age in days (0 = no limit)
        /// </summary>
        public int MaxPasswordAgeDays { get; set; } = 90;

        /// <summary>
        /// Number of previous passwords to remember (prevent reuse)
        /// </summary>
        public int PasswordHistoryCount { get; set; } = 5;
    }

    /// <summary>
    /// Account lockout policy
    /// </summary>
    public class AccountLockoutPolicy
    {
        /// <summary>
        /// Maximum failed login attempts before lockout
        /// </summary>
        public int MaxFailedLoginAttempts { get; set; } = 5;

        /// <summary>
        /// Lockout duration in minutes
        /// </summary>
        public int LockoutDurationMinutes { get; set; } = 15;

        /// <summary>
        /// Enable account lockout
        /// </summary>
        public bool IsEnabled { get; set; } = true;
    }

    /// <summary>
    /// Authentication configuration container
    /// </summary>
    public class AuthenticationSettings
    {
        public JwtSettings Jwt { get; set; } = new();
        public PasswordPolicy PasswordPolicy { get; set; } = new();
        public AccountLockoutPolicy AccountLockout { get; set; } = new();

        /// <summary>
        /// Enable multi-factor authentication
        /// </summary>
        public bool EnableMfa { get; set; } = false;

        /// <summary>
        /// Session timeout in minutes
        /// </summary>
        public int SessionTimeoutMinutes { get; set; } = 30;

        /// <summary>
        /// Enable IP-based access control
        /// </summary>
        public bool EnableIpRestriction { get; set; } = false;

        /// <summary>
        /// Allowed IP addresses (comma-separated) - if EnableIpRestriction is true
        /// </summary>
        public string AllowedIpAddresses { get; set; } = string.Empty;
    }
}
