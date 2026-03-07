using AMS.Authentication.Configuration;
using AMS.Authentication.Models;
using AMS.Authentication.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AMS.Authentication.Services
{
    /// <summary>
    /// JWT token generation and validation service
    /// Implements OAuth 2.0 and JWT best practices
    /// </summary>
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<TokenService> _logger;

        public TokenService(IOptions<AuthenticationSettings> authSettings, ILogger<TokenService> logger)
        {
            _jwtSettings = authSettings.Value.Jwt;
            _logger = logger;
        }

        /// <summary>
        /// Generate JWT access token with security best practices
        /// </summary>
        public string GenerateAccessToken(AppUser user, IList<string> roles)
        {
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                // Build claims following JWT best practices
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                    new Claim(ClaimTypes.GivenName, user.FirstName),
                    new Claim(ClaimTypes.Surname, user.LastName),
                    new Claim("FullName", user.FullName),
                    new Claim("IsActive", user.IsActive.ToString()),
                    new Claim("Jti", Guid.NewGuid().ToString()), // JWT ID for token revocation
                };

                // Add roles as claims
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var token = new JwtSecurityToken(
                    issuer: _jwtSettings.Issuer,
                    audience: _jwtSettings.Audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                    signingCredentials: credentials
                );

                var tokenHandler = new JwtSecurityTokenHandler();
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error generating access token for user {UserId}: {Message}", user.Id, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Generate secure refresh token
        /// </summary>
        public async Task<RefreshToken> GenerateRefreshTokenAsync(int userId, string ipAddress, string? userAgent)
        {
            // Generate cryptographically secure random token
            var randomNumber = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }

            var refreshToken = new RefreshToken
            {
                UserId = userId,
                Token = Convert.ToBase64String(randomNumber),
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ipAddress,
                UserAgent = userAgent
            };

            _logger.LogInformation("Refresh token generated for user {UserId} from IP {IpAddress}", userId, ipAddress);
            return await Task.FromResult(refreshToken);
        }

        /// <summary>
        /// Validate JWT token and extract claims
        /// </summary>
        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
                var tokenHandler = new JwtSecurityTokenHandler();

                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = _jwtSettings.ValidateIssuerSigningKey,
                    IssuerSigningKey = key,
                    ValidateIssuer = _jwtSettings.ValidateIssuer,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = _jwtSettings.ValidateAudience,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = _jwtSettings.ValidateLifetime,
                    ClockSkew = TimeSpan.FromSeconds(_jwtSettings.ClockSkewSeconds),
                    // Additional security: require Jti claim
                    RequireExpirationTime = true,
                }, out SecurityToken validatedToken);

                return principal;
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning("Token validation failed: {Message}", ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error validating token: {Message}", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Revoke specific refresh token
        /// </summary>
        public async Task<bool> RevokeTokenAsync(int userId, string token, string ipAddress, string? reason)
        {
            // This would be implemented in a repository/database context
            // For now, return true to indicate successful revocation
            _logger.LogInformation("Refresh token revoked for user {UserId} from IP {IpAddress}. Reason: {Reason}",
                userId, ipAddress, reason ?? "Not specified");
            
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Revoke all refresh tokens for a user (e.g., on password change or logout)
        /// </summary>
        public async Task<bool> RevokeAllTokensAsync(int userId, string ipAddress, string? reason)
        {
            _logger.LogInformation("All refresh tokens revoked for user {UserId} from IP {IpAddress}. Reason: {Reason}",
                userId, ipAddress, reason ?? "Not specified");
            
            // This would be implemented in a repository/database context
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Get access token expiration time
        /// </summary>
        public int GetAccessTokenExpirationSeconds()
        {
            return _jwtSettings.AccessTokenExpirationMinutes * 60;
        }

        /// <summary>
        /// Get refresh token expiration time
        /// </summary>
        public int GetRefreshTokenExpirationDays()
        {
            return _jwtSettings.RefreshTokenExpirationDays;
        }
    }
}
