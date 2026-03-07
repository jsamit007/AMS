namespace AMS.Authentication.DTOs
{
    /// <summary>
    /// User login request DTO
    /// </summary>
    public class LoginRequestDto
    {
        /// <summary>
        /// User's email address
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// User's password
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Whether to remember this login device
        /// </summary>
        public bool RememberMe { get; set; } = false;

        /// <summary>
        /// IP address of the login request (for audit trail)
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// User agent/device information (for audit trail)
        /// </summary>
        public string? UserAgent { get; set; }
    }

    /// <summary>
    /// User registration request DTO
    /// </summary>
    public class RegisterRequestDto
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
        /// Email address (used as username)
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Password (must meet complexity requirements)
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Password confirmation
        /// </summary>
        public string ConfirmPassword { get; set; } = string.Empty;

        /// <summary>
        /// Employee ID if registering existing employee
        /// </summary>
        public int? EmployeeId { get; set; }

        /// <summary>
        /// IP address of the registration request (for audit trail)
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// User agent/device information (for audit trail)
        /// </summary>
        public string? UserAgent { get; set; }
    }

    /// <summary>
    /// Authentication response containing tokens and user info
    /// </summary>
    public class AuthenticationResponseDto
    {
        /// <summary>
        /// JWT access token (short-lived, typically 15 minutes)
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Refresh token (long-lived, typically 7 days)
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// Access token expiration time in seconds
        /// </summary>
        public int ExpiresIn { get; set; }

        /// <summary>
        /// Token type (always "Bearer" for JWT)
        /// </summary>
        public string TokenType { get; set; } = "Bearer";

        /// <summary>
        /// Authenticated user information
        /// </summary>
        public UserInfoDto User { get; set; } = new();
    }

    /// <summary>
    /// Refresh token request DTO
    /// </summary>
    public class RefreshTokenRequestDto
    {
        /// <summary>
        /// The refresh token to use for getting a new access token
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// IP address of the refresh request (for audit trail)
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// User agent/device information (for audit trail)
        /// </summary>
        public string? UserAgent { get; set; }
    }

    /// <summary>
    /// User information DTO (safe to expose to client)
    /// </summary>
    public class UserInfoDto
    {
        /// <summary>
        /// User ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Email address
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// User's first name
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// User's last name
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// User's full name
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// User's assigned roles
        /// </summary>
        public IList<string> Roles { get; set; } = new List<string>();

        /// <summary>
        /// User's permissions
        /// </summary>
        public IList<string> Permissions { get; set; } = new List<string>();

        /// <summary>
        /// Whether the user account is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Last login timestamp
        /// </summary>
        public DateTime? LastLoginAt { get; set; }
    }

    /// <summary>
    /// Change password request DTO
    /// </summary>
    public class ChangePasswordRequestDto
    {
        /// <summary>
        /// Current password for verification
        /// </summary>
        public string CurrentPassword { get; set; } = string.Empty;

        /// <summary>
        /// New password
        /// </summary>
        public string NewPassword { get; set; } = string.Empty;

        /// <summary>
        /// Confirm new password
        /// </summary>
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// Logout request DTO
    /// </summary>
    public class LogoutRequestDto
    {
        /// <summary>
        /// IP address of the logout request
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// Reason for logout (optional)
        /// </summary>
        public string? Reason { get; set; }
    }
}
