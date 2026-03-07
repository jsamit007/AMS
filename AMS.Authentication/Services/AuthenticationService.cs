using AMS.Authentication.Configuration;
using AMS.Authentication.DTOs;
using AMS.Authentication.Models;
using AMS.Authentication.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AMS.Authentication.Services
{
    /// <summary>
    /// Authentication service for user login, registration, and token management
    /// Implements industry-level security practices
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly ITokenService _tokenService;
        private readonly AuthenticationSettings _authSettings;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager,
            ITokenService tokenService,
            IOptions<AuthenticationSettings> authSettings,
            ILogger<AuthenticationService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
            _authSettings = authSettings.Value;
            _logger = logger;
        }

        /// <summary>
        /// Authenticate user with email and password
        /// Implements security best practices including account lockout
        /// </summary>
        public async Task<AuthenticationResponseDto?> AuthenticateAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Login attempt for user: {Email} from IP: {IpAddress}", request.Email, request.IpAddress);

            // Validate input
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogWarning("Login attempt with empty email or password from IP: {IpAddress}", request.IpAddress);
                return null;
            }

            // Find user by email (case-insensitive)
            var user = await _userManager.FindByEmailAsync(request.Email.ToLower());
            if (user == null)
            {
                _logger.LogWarning("Login attempt for non-existent user: {Email}", request.Email);
                return null;
            }

            // Check if account is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Login attempt for inactive user: {Email}", request.Email);
                return null;
            }

            // Check if account is locked
            if (await _userManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning("Login attempt for locked user: {Email}", request.Email);
                return null;
            }

            // Verify password
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
            {
                // For lockout: track in AppUser.IsLocked property
                // In production, implement a FailedAttempt table for better tracking
                if (_authSettings.AccountLockout.IsEnabled)
                {
                    user.IsLocked = true;
                    var lockoutEnd = DateTimeOffset.UtcNow.AddMinutes(_authSettings.AccountLockout.LockoutDurationMinutes);
                    await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);
                    await _userManager.UpdateAsync(user);
                    _logger.LogWarning("User account locked due to failed login attempt: {Email}", request.Email);
                }

                _logger.LogWarning("Invalid password for user: {Email} from IP: {IpAddress}", request.Email, request.IpAddress);
                return null;
            }

            // Reset lockout on successful authentication
            user.IsLocked = false;
            await _userManager.SetLockoutEndDateAsync(user, null);
            await _userManager.UpdateAsync(user);

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            // Generate tokens
            var accessToken = _tokenService.GenerateAccessToken(user, roles);
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync(
                user.Id,
                request.IpAddress ?? "Unknown",
                request.UserAgent);

            // Get user info
            var userInfo = new UserInfoDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Roles = roles,
                IsActive = user.IsActive,
                LastLoginAt = user.LastLoginAt
            };

            _logger.LogInformation("Successful login for user: {Email} from IP: {IpAddress}", request.Email, request.IpAddress);

            return new AuthenticationResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                ExpiresIn = _tokenService.GetAccessTokenExpirationSeconds(),
                TokenType = "Bearer",
                User = userInfo
            };
        }

        /// <summary>
        /// Register a new user with password complexity validation
        /// </summary>
        public async Task<(bool Success, string? Message)> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Registration request for email: {Email}", request.Email);

            // Validate input
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return (false, "Email and password are required");
            }

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email.ToLower());
            if (existingUser != null)
            {
                _logger.LogWarning("Registration attempt with existing email: {Email}", request.Email);
                return (false, "Email already registered");
            }

            // Validate password confirmation
            if (request.Password != request.ConfirmPassword)
            {
                return (false, "Passwords do not match");
            }

            // Create new user
            var user = new AppUser
            {
                UserName = request.Email.ToLower(),
                Email = request.Email.ToLower(),
                EmailConfirmed = false, // Require email confirmation in production
                FirstName = request.FirstName,
                LastName = request.LastName,
                EmployeeId = request.EmployeeId,
                CreatedAt = DateTime.UtcNow
            };

            // Create user with password
            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("User creation failed for email {Email}: {Errors}", request.Email, errors);
                return (false, $"Registration failed: {errors}");
            }

            // Assign default role if exists
            var defaultRole = await _roleManager.FindByNameAsync("User");
            if (defaultRole != null)
            {
                await _userManager.AddToRoleAsync(user, defaultRole.Name ?? "User");
            }

            _logger.LogInformation("User registered successfully: {Email}", request.Email);
            return (true, null);
        }

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        public async Task<AuthenticationResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Token refresh request from IP: {IpAddress}", request.IpAddress);

            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return null;
            }

            // In a real implementation, validate the refresh token from database
            // For now, this is a placeholder that should be implemented with data access

            return null;
        }

        /// <summary>
        /// Logout user and optionally revoke refresh tokens
        /// </summary>
        public async Task<bool> LogoutAsync(int userId, LogoutRequestDto request, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return false;
            }

            // Revoke all refresh tokens
            await _tokenService.RevokeAllTokensAsync(userId, request.IpAddress ?? "Unknown", request.Reason);

            _logger.LogInformation("User {UserId} logged out from IP: {IpAddress}", userId, request.IpAddress);
            return true;
        }

        /// <summary>
        /// Change user password with validation
        /// </summary>
        public async Task<(bool Success, string? Message)> ChangePasswordAsync(int userId, ChangePasswordRequestDto request, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return (false, "User not found");
            }

            // Validate password confirmation
            if (request.NewPassword != request.ConfirmPassword)
            {
                return (false, "New passwords do not match");
            }

            // Verify current password
            var isCurrentPasswordValid = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);
            if (!isCurrentPasswordValid)
            {
                _logger.LogWarning("Invalid current password attempted for user: {UserId}", userId);
                return (false, "Current password is incorrect");
            }

            // Change password
            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return (false, $"Password change failed: {errors}");
            }

            // Update last password change date
            user.LastPasswordChangeAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Revoke all tokens to require re-authentication
            await _tokenService.RevokeAllTokensAsync(userId, "Unknown", "Password changed");

            _logger.LogInformation("Password changed for user: {UserId}", userId);
            return (true, null);
        }

        /// <summary>
        /// Get user information
        /// </summary>
        public async Task<UserInfoDto?> GetUserInfoAsync(int userId, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);

            return new UserInfoDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Roles = roles,
                IsActive = user.IsActive,
                LastLoginAt = user.LastLoginAt
            };
        }

        /// <summary>
        /// Check if user email exists
        /// </summary>
        public async Task<bool> UserExistsAsync(string email, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(email.ToLower());
            return user != null;
        }

        /// <summary>
        /// Request password reset
        /// </summary>
        public async Task<(bool Success, string? Token)> RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(email.ToLower());
            if (user == null)
            {
                // Don't reveal if email exists (security best practice)
                _logger.LogInformation("Password reset requested for non-existent email: {Email}", email);
                return (true, null);
            }

            // Generate password reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            _logger.LogInformation("Password reset token generated for user: {Email}", email);
            return (true, token);
        }

        /// <summary>
        /// Reset password using token
        /// </summary>
        public async Task<(bool Success, string? Message)> ResetPasswordAsync(string token, string newPassword, CancellationToken cancellationToken = default)
        {
            // This would require additional parameters to identify the user
            // Typically: email + new password + token
            return (false, "Not implemented");
        }
    }
}
