using AMS.Authentication.Models;
using System.Security.Claims;

namespace AMS.Authentication.Services.Interfaces
{
    /// <summary>
    /// Interface for JWT token operations
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generate JWT access token
        /// </summary>
        string GenerateAccessToken(AppUser user, IList<string> roles);

        /// <summary>
        /// Generate refresh token
        /// </summary>
        Task<RefreshToken> GenerateRefreshTokenAsync(int userId, string ipAddress, string? userAgent);

        /// <summary>
        /// Validate JWT token and extract claims
        /// </summary>
        ClaimsPrincipal? ValidateToken(string token);

        /// <summary>
        /// Revoke refresh token
        /// </summary>
        Task<bool> RevokeTokenAsync(int userId, string token, string ipAddress, string? reason);

        /// <summary>
        /// Revoke all user tokens
        /// </summary>
        Task<bool> RevokeAllTokensAsync(int userId, string ipAddress, string? reason);

        /// <summary>
        /// Get access token expiration time in seconds
        /// </summary>
        int GetAccessTokenExpirationSeconds();

        /// <summary>
        /// Get refresh token expiration time in days
        /// </summary>
        int GetRefreshTokenExpirationDays();
    }
}
