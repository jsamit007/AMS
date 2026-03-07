# AMS.Authentication - Attendance Management System Authentication Module

## Overview

AMS.Authentication is an industry-level authentication and authorization module for the Attendance Management System. It implements best practices for:

- **JWT (JSON Web Token) based authentication** with secure token generation and validation
- **Role-Based Access Control (RBAC)** for flexible permission management
- **Claims-based authorization** for fine-grained access control
- **Refresh token mechanism** for secure token renewal
- **Account security features** including:
  - Password complexity requirements
  - Account lockout after failed attempts
  - Password change enforcement
  - Secure password hashing using Identity framework
  - IP address and user agent tracking
  - Audit logging of all authentication events

## Architecture

### Models

- **AppUser** - Extended IdentityUser with custom properties:
  - First/Last name, Employee ID reference
  - Account status and lock status
  - Last login timestamp
  - Password change tracking
  - Refresh token history

- **AppRole** - Extended IdentityRole with custom properties:
  - Role description and priority
  - System role flag (prevent deletion)
  - Permission collection

- **RefreshToken** - OAuth 2.0 refresh token model:
  - Cryptographically secure token storage
  - Token expiration and revocation tracking
  - IP address and user agent logging
  - Revocation reason tracking

- **RolePermission** - Fine-grained permission model:
  - Permission format: "resource:action" (e.g., "attendance:create")
  - Active/inactive status
  - Permission assignments by role

### Services

#### IAuthenticationService
Handles user authentication, registration, and account management:
- `AuthenticateAsync()` - Login with email/password
- `RegisterAsync()` - User registration with validation
- `RefreshTokenAsync()` - Refresh expired access tokens
- `LogoutAsync()` - Logout and token revocation
- `ChangePasswordAsync()` - Secure password change
- `RequestPasswordResetAsync()` - Password reset token generation
- `ResetPasswordAsync()` - Password reset via token

#### ITokenService
Manages JWT and refresh tokens:
- `GenerateAccessToken()` - Create JWT access token with claims
- `GenerateRefreshTokenAsync()` - Create secure refresh token
- `ValidateToken()` - Validate JWT and extract claims
- `RevokeTokenAsync()` - Revoke specific refresh token
- `RevokeAllTokensAsync()` - Revoke all user tokens (security event)

#### IRoleService
Manages roles and permissions:
- `GetAllRolesAsync()` - List all roles
- `CreateRoleAsync()` - Create new role
- `AssignPermissionAsync()` - Add permission to role
- `AssignRoleToUserAsync()` - Assign role to user
- `GetUserPermissionsAsync()` - Get all user permissions

## Configuration

### appsettings.json Configuration

```json
{
  "Authentication": {
    "Jwt": {
      "SecretKey": "your-256-bit-secret-key-minimum-32-characters-long",
      "Issuer": "AMS",
      "Audience": "AMSUsers",
      "AccessTokenExpirationMinutes": 15,
      "RefreshTokenExpirationDays": 7,
      "RequireHttpsMetadata": true,
      "ValidateIssuer": true,
      "ValidateAudience": true,
      "ValidateIssuerSigningKey": true,
      "ValidateLifetime": true,
      "ClockSkewSeconds": 60
    },
    "PasswordPolicy": {
      "MinimumLength": 8,
      "RequireUppercase": true,
      "RequireLowercase": true,
      "RequireDigits": true,
      "RequireNonAlphanumericCharacters": true,
      "MaxPasswordAgeDays": 90,
      "PasswordHistoryCount": 5
    },
    "AccountLockout": {
      "MaxFailedLoginAttempts": 5,
      "LockoutDurationMinutes": 15,
      "IsEnabled": true
    },
    "EnableMfa": false,
    "SessionTimeoutMinutes": 30,
    "EnableIpRestriction": false,
    "AllowedIpAddresses": ""
  }
}
```

## Usage

### Setup in Startup (Program.cs)

```csharp
using AMS.Authentication.Extensions;
using AMS.Repository.Models;

var builder = WebApplicationBuilder.CreateBuilder(args);

// Configure Authentication Services
builder.Services.AddAuthenticationServices(builder.Configuration);
builder.Services.AddAuthorizationPolicies();

// Configure Identity
builder.Services.AddIdentity<AppUser, AppRole>(options =>
{
    // Password complexity options
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    
    // Account lockout options
    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<AttendanceManagementContext>()
.AddDefaultTokenProviders();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
```

### API Endpoints Examples

#### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePass123!",
  "rememberMe": true
}
```

Response:
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64encodedtoken...",
  "expiresIn": 900,
  "tokenType": "Bearer",
  "user": {
    "id": 1,
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "fullName": "John Doe",
    "roles": ["User", "Manager"],
    "permissions": ["attendance:read", "attendance:create"],
    "isActive": true,
    "lastLoginAt": "2026-03-08T10:30:00Z"
  }
}
```

#### Register
```http
POST /api/auth/register
Content-Type: application/json

{
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "jane.smith@example.com",
  "password": "SecurePass123!",
  "confirmPassword": "SecurePass123!",
  "employeeId": 123
}
```

#### Refresh Token
```http
POST /api/auth/refresh
Content-Type: application/json

{
  "refreshToken": "base64encodedtoken..."
}
```

#### Change Password
```http
POST /api/auth/change-password
Authorization: Bearer <accessToken>
Content-Type: application/json

{
  "currentPassword": "OldPass123!",
  "newPassword": "NewPass456!",
  "confirmPassword": "NewPass456!"
}
```

### Protected Controller Example

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication
public class AttendanceController : ControllerBase
{
    [HttpGet("{id}")]
    [Authorize(Policy = "CanViewAttendance")]
    public async Task<IActionResult> GetAttendance(int id)
    {
        // Only users with "attendance:read" permission can access
        return Ok();
    }

    [HttpPost]
    [Authorize(Policy = "CanCreateAttendance")]
    public async Task<IActionResult> CreateAttendance(CreateAttendanceDto dto)
    {
        // Only users with "attendance:create" permission can access
        return Created();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")] // Admin role requirement
    public async Task<IActionResult> DeleteAttendance(int id)
    {
        // Only Admin role users can access
        return NoContent();
    }
}
```

## Security Best Practices Implemented

### 1. **Token Security**
- Short-lived access tokens (15 minutes default)
- Long-lived refresh tokens with separate storage
- Secure random token generation using `RandomNumberGenerator`
- Token revocation tracking
- Clock skew tolerance for distributed systems

### 2. **Password Security**
- Bcrypt hashing via ASP.NET Identity
- Complexity requirements (uppercase, lowercase, digits, special chars)
- Password history to prevent reuse
- Password age enforcement (expiration)
- One-way hashing (irreversible)

### 3. **Account Security**
- Account lockout after failed attempts
- Progressive lockout duration
- Account active/locked status tracking
- Session timeout enforcement
- Last login timestamp for audit

### 4. **Audit & Logging**
- All authentication attempts logged
- Failed login tracking
- IP address and user agent recording
- Password changes logged
- Token revocation reasons tracked

### 5. **API Security**
- JWT signature validation
- Token lifetime validation
- Claims-based authorization
- Role-based access control
- Permission-based access control

### 6. **HTTPS Enforcement**
- Metadata requires HTTPS in production
- Secure token transmission
- No tokens in URLs

## Default Roles (to be created during application startup)

```csharp
// Admin - Full system access
var adminRole = new AppRole 
{ 
    Name = "Admin", 
    Description = "Administrator with full system access",
    Priority = 100,
    IsSystemRole = true
};

// Manager - Department/team management
var managerRole = new AppRole 
{ 
    Name = "Manager", 
    Description = "Manager with team oversight",
    Priority = 50,
    IsSystemRole = true
};

// User - Regular employee
var userRole = new AppRole 
{ 
    Name = "User", 
    Description = "Regular user/employee",
    Priority = 10,
    IsSystemRole = true
};
```

## Standard Permissions (to be assigned to roles)

```
Attendance Permissions:
- attendance:read - View attendance records
- attendance:create - Create attendance records
- attendance:update - Update attendance records
- attendance:delete - Delete attendance records
- attendance:approve - Approve attendance

Leave Permissions:
- leave:read - View leave requests
- leave:create - Create leave requests
- leave:approve - Approve leave requests

Reports Permissions:
- reports:read - View reports
- reports:export - Export reports
- reports:email - Email reports

User Management Permissions:
- users:read - View users
- users:create - Create users
- users:update - Update users
- users:delete - Delete users

Role Management Permissions:
- roles:read - View roles
- roles:create - Create roles
- roles:update - Update roles
- roles:delete - Delete roles
```

## Testing

### Unit Tests for Authentication

```csharp
[Fact]
public async Task AuthenticateAsync_WithValidCredentials_ReturnsAuthenticationResponse()
{
    // Arrange
    var request = new LoginRequestDto 
    { 
        Email = "test@example.com", 
        Password = "ValidPass123!" 
    };
    
    // Act
    var result = await _authService.AuthenticateAsync(request);
    
    // Assert
    Assert.NotNull(result);
    Assert.NotNull(result.AccessToken);
    Assert.NotNull(result.RefreshToken);
    Assert.Equal(request.Email, result.User.Email);
}

[Fact]
public async Task AuthenticateAsync_WithInvalidPassword_ReturnsNull()
{
    // Arrange
    var request = new LoginRequestDto 
    { 
        Email = "test@example.com", 
        Password = "WrongPassword" 
    };
    
    // Act
    var result = await _authService.AuthenticateAsync(request);
    
    // Assert
    Assert.Null(result);
}
```

## Migration & Database Setup

The authentication system integrates with Entity Framework Core and uses the same database as the main application. Ensure your DbContext is configured with:

```csharp
public class AttendanceManagementContext : IdentityDbContext<AppUser, AppRole, int>
{
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    
    // ... other DbSets
}
```

Run migrations:
```bash
dotnet ef migrations add AddAuthenticationTables
dotnet ef database update
```

## Troubleshooting

### Issue: "Invalid token"
**Solution**: Check token hasn't expired, JWT secret key is correct, and issuer/audience match configuration

### Issue: "User account is locked"
**Solution**: Wait for lockout duration to expire or manually unlock user in database

### Issue: "Password does not meet complexity requirements"
**Solution**: Ensure password contains uppercase, lowercase, digits, and special characters

### Issue: "Refresh token expired"
**Solution**: User must log in again; refresh tokens have fixed expiration (7 days default)

## Production Checklist

- [ ] Use strong secret key (minimum 32 characters)
- [ ] Store secret key in Azure Key Vault or environment variables
- [ ] Enable HTTPS requirement
- [ ] Configure appropriate token expiration times
- [ ] Set up proper logging and monitoring
- [ ] Configure database backups
- [ ] Test token revocation mechanism
- [ ] Implement MFA for sensitive accounts
- [ ] Set up IP restrictions if applicable
- [ ] Regular security audits and penetration testing
- [ ] Monitor failed login attempts
- [ ] Implement rate limiting on authentication endpoints

## References

- [Microsoft Identity Documentation](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/)
- [JWT Best Practices](https://tools.ietf.org/html/rfc8725)
- [OAuth 2.0 Security](https://datatracker.ietf.org/doc/html/draft-ietf-oauth-security-topics)
- [OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)
