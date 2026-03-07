using AMS.Authentication.DTOs;

namespace AMS.Authentication.Services.Interfaces
{
    /// <summary>
    /// Interface for authentication operations
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Authenticate user with email and password
        /// </summary>
        Task<AuthenticationResponseDto?> AuthenticateAsync(LoginRequestDto request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Register a new user
        /// </summary>
        Task<(bool Success, string? Message)> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        Task<AuthenticationResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Logout user and revoke refresh tokens
        /// </summary>
        Task<bool> LogoutAsync(int userId, LogoutRequestDto request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Change user password
        /// </summary>
        Task<(bool Success, string? Message)> ChangePasswordAsync(int userId, ChangePasswordRequestDto request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get user information by ID
        /// </summary>
        Task<UserInfoDto?> GetUserInfoAsync(int userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verify if user email exists
        /// </summary>
        Task<bool> UserExistsAsync(string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Request password reset
        /// </summary>
        Task<(bool Success, string? Token)> RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Reset password using token
        /// </summary>
        Task<(bool Success, string? Message)> ResetPasswordAsync(string token, string newPassword, CancellationToken cancellationToken = default);
    }
}
