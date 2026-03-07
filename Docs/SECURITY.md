# Security - AMS

Comprehensive security documentation covering authentication, authorization, and best practices.

## Overview

The AMS implements industry-standard security practices including JWT authentication, role-based access control, and encrypted sensitive data.

## Authentication

### JWT (JSON Web Tokens)

The API uses JWT Bearer tokens for stateless authentication.

#### Token Structure

```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiZW1haWwiOiJ1c2VyQGV4YW1wbGUuY29tIiwicm9sZXMiOlsiRW1wbG95ZWUiXSwiaWF0IjoxNjE2MjM5MDIyLCJleHAiOjE2MTYyNDI2MjJ9.signature
^-- Header --^.^--- Payload ---^.^-- Signature --^
```

**Header** (Base64 decoded):
```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

**Payload** (Base64 decoded):
```json
{
  "sub": "1",
  "email": "user@example.com",
  "name": "John Doe",
  "roles": ["Employee"],
  "iat": 1616239022,
  "exp": 1616242622
}
```

**Signature**:
```
HMACSHA256(
  base64UrlEncode(header) + "." +
  base64UrlEncode(payload),
  secret_key
)
```

#### Claims in Token

| Claim | Type | Purpose |
|-------|------|---------|
| `sub` | String | User ID (subject) |
| `email` | String | User email address |
| `name` | String | Full name |
| `roles` | Array | User roles (Admin, Manager, Employee, HR) |
| `iat` | Number | Issued at (Unix timestamp) |
| `exp` | Number | Expiration (Unix timestamp) |
| `permissions` | Array | Specific permissions |

#### Token Lifecycle

1. **Generation**: User logs in with credentials
2. **Validation**: Server validates credentials
3. **Issuance**: Server generates JWT token
4. **Transmission**: Client receives token
5. **Usage**: Client includes token in Authorization header
6. **Verification**: Server verifies signature and expiration
7. **Expiration**: Token expires (default: 1 hour)
8. **Refresh**: User must re-authenticate (no refresh token currently)

### Login Endpoint

**Endpoint**: `POST /api/auth/login`

**Request**:
```json
{
  "email": "john.doe@company.com",
  "password": "SecurePassword123!"
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresIn": 3600,
    "user": {
      "id": 1,
      "email": "john.doe@company.com",
      "firstName": "John",
      "lastName": "Doe",
      "roles": ["Employee"]
    }
  }
}
```

**Response** (401 Unauthorized):
```json
{
  "success": false,
  "message": "Invalid email or password",
  "errors": []
}
```

### Token Configuration

Configure in `appsettings.json`:

```json
{
  "Jwt": {
    "SecretKey": "your-super-secret-key-minimum-32-characters-long",
    "Issuer": "ams-issuer",
    "Audience": "ams-audience",
    "ExpirationMinutes": 60
  }
}
```

**Best Practices**:
- ✅ Use strong secrets (minimum 32 characters)
- ✅ Use environment variables in production
- ✅ Rotate secrets regularly
- ✅ Use HTTPS for token transmission
- ❌ Don't hardcode secrets in source code
- ❌ Don't transmit tokens in URLs
- ❌ Don't store tokens in plain text

### JWT Validation

The middleware validates:
- ✅ Token signature matches secret key
- ✅ Token hasn't expired
- ✅ Token was issued by correct issuer
- ✅ Token is for correct audience
- ✅ Token format is valid

## Authorization

### Role-Based Access Control (RBAC)

Four predefined roles:

| Role | Description | Permissions |
|------|-------------|------------|
| `Admin` | System administrator | Full access to all operations |
| `Manager` | Department manager | View/manage own department, approve leave |
| `HR` | Human resources | Manage employees, approve leave, reports |
| `Employee` | Regular employee | View own data, request leave |

### Authorization Policies

Configure in `Program.cs`:

```csharp
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => 
        policy.RequireRole("Admin"))
    .AddPolicy("ManagerOrAbove", policy => 
        policy.RequireRole("Admin", "Manager"))
    .AddPolicy("CanApproveLeave", policy => 
        policy.RequireRole("Admin", "Manager", "HR"))
    .AddPolicy("OwnResourceOrAdmin", policy => 
        policy.RequireAssertion(context => 
        {
            var userId = context.User.FindFirst("sub")?.Value;
            var requestUserId = context.Resource?.ToString();
            return context.User.IsInRole("Admin") || 
                   userId == requestUserId;
        }));
```

### Endpoint Authorization

Protect endpoints with `[Authorize]` and `[Authorize(Roles="...")]`:

```csharp
[ApiController]
[Route("api/[controller]")]
public class EmployeeController : ControllerBase
{
    // Public endpoint (no auth required)
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        // ...
    }

    // Requires any authenticated user
    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        // ...
    }

    // Requires Admin or HR role
    [Authorize(Roles = "Admin,HR")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateEmployeeRequest request)
    {
        // ...
    }

    // Requires Admin role only
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        // ...
    }

    // Use custom policy
    [Authorize(Policy = "OwnResourceOrAdmin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateEmployeeRequest request)
    {
        // ...
    }
}
```

### Context-Based Authorization

Check roles in code:

```csharp
if (User.IsInRole("Admin"))
{
    // Admin-only logic
}

if (User.HasClaim("permissions", "approve_leave"))
{
    // Show approve button
}

var userId = User.FindFirst("sub")?.Value;
if (userId == resourceOwnerId || User.IsInRole("Admin"))
{
    // Allow edit
}
```

## Data Protection

### Password Hashing

Passwords are hashed using ASP.NET Core Identity with bcrypt:

```csharp
var hasher = new PasswordHasher<User>();
var hash = hasher.HashPassword(user, "password123");
var result = hasher.VerifyHashedPassword(user, hash, "password123");
```

**Never**:
- ❌ Store plain-text passwords
- ❌ Use MD5 or SHA1 for passwords
- ❌ Log passwords
- ❌ Transmit unhashed passwords over HTTP

### Encryption

Sensitive data can be encrypted:

```csharp
// Configuration
var dataProtectionProvider = DataProtectionProvider.Create("AMS");
var protector = dataProtectionProvider.CreateProtector("AMS.Security");

// Encrypt
string encrypted = protector.Protect("sensitive data");

// Decrypt
string decrypted = protector.Unprotect(encrypted);
```

## CORS (Cross-Origin Resource Sharing)

CORS configuration prevents unauthorized cross-origin requests.

### Configure CORS

In `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3000",
                "http://localhost:5173",
                "https://yourdomain.com"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

app.UseCors("AllowSpecificOrigins");
```

### CORS Policy Configuration

See `appsettings.{Environment}.json`:

```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "https://yourdomain.com"
    ]
  }
}
```

## HTTPS/TLS

Enforce HTTPS for all communication:

```csharp
app.UseHttpsRedirection();
app.UseHsts(); // In production
```

### Certificate Configuration

Launch settings for HTTPS:

```json
{
  "https": {
    "url": "https://localhost:7001",
    "certificate": {
      "path": "mycert.pfx",
      "password": "password"
    }
  }
}
```

## Rate Limiting

Prevent abuse and DDoS attacks:

**Configuration** (appsettings.json):
```json
{
  "RateLimiting": {
    "MaxRequests": 100,
    "TimeWindowSeconds": 60
  }
}
```

**How it works**:
- Tracks requests per IP address
- Returns 429 Too Many Requests when limit exceeded
- Resets counter after time window

## Security Headers

Configure security headers in middleware:

```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000";
    context.Response.Headers["Content-Security-Policy"] = "default-src 'self'";
    await next();
});
```

## Input Validation

Validate all user input using FluentValidation:

```csharp
public class CreateEmployeeValidator : AbstractValidator<CreateEmployeeRequest>
{
    public CreateEmployeeValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(50).WithMessage("First name too long");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.EmployeeCode)
            .NotEmpty().WithMessage("Employee code is required")
            .Matches("^[A-Z0-9]+$").WithMessage("Invalid format");
    }
}
```

## SQL Injection Prevention

The application is protected from SQL injection:

**✅ Safe** (Using Entity Framework Core with parameterized queries):
```csharp
var employee = await context.Employees
    .FirstOrDefaultAsync(e => e.EmployeeCode == employeeCode);
```

**❌ Unsafe** (String concatenation):
```csharp
var sql = $"SELECT * FROM employee WHERE code = '{employeeCode}'"; // NEVER DO THIS!
```

## Secrets Management

### Local Development

Use User Secrets:

```bash
# Initialize
dotnet user-secrets init

# Set secret
dotnet user-secrets set "Jwt:SecretKey" "your-secret-key-here"

# List secrets
dotnet user-secrets list

# Remove secret
dotnet user-secrets remove "Jwt:SecretKey"
```

### Production

Use AWS Secrets Manager:

```csharp
builder.Configuration.AddSecretsManager(region: "us-east-1");
```

Or environment variables:

```bash
export Jwt__SecretKey=your-production-secret-key
export ConnectionStrings__DefaultConnection=your-db-connection-string
```

## Audit Logging

Log security-relevant events:

```csharp
_logger.LogInformation(
    "User {UserId} accessed {Resource} at {Timestamp}",
    userId, resource, DateTime.UtcNow
);

_logger.LogWarning(
    "Failed login attempt for {Email} from {IpAddress}",
    email, ipAddress
);

_logger.LogError(
    "Unauthorized access attempt: {UserId} tried to access {Resource}",
    userId, resource
);
```

## GDPR & Data Privacy

### Data Retention

Implement data retention policies:

```csharp
public async Task DeleteOldLeaveRecords(int daysToKeep)
{
    var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
    var oldRecords = await context.Leaves
        .Where(l => l.CreatedDate < cutoffDate && l.Status == "Completed")
        .ToListAsync();
    
    context.Leaves.RemoveRange(oldRecords);
    await context.SaveChangesAsync();
}
```

### Right to be Forgotten

Implement data deletion endpoint:

```csharp
[Authorize]
[HttpDelete("me")]
public async Task<IActionResult> DeleteMyData()
{
    var userId = User.FindFirst("sub")?.Value;
    
    // Delete personal data
    var employee = await _unitOfWork.Employees.GetByIdAsync(int.Parse(userId));
    await _unitOfWork.Employees.DeleteAsync(employee);
    await _unitOfWork.SaveChangesAsync();
    
    return Ok(new { message = "Your data has been deleted" });
}
```

## Security Checklist

- [ ] HTTPS enabled in production
- [ ] JWT secret key is strong and random
- [ ] Passwords hashed with bcrypt
- [ ] CORS properly configured
- [ ] Rate limiting enabled
- [ ] Input validation on all endpoints
- [ ] Authorization policies enforced
- [ ] Sensitive data not logged
- [ ] Secret data not in version control
- [ ] Regular security updates/patching
- [ ] SQL injection prevention verified
- [ ] XSS protection headers configured
- [ ] CSRF protection for state-changing operations
- [ ] TLS 1.2+ enforced
- [ ] Security audit logs maintained

## Vulnerability Disclosure

If you discover a security vulnerability:

1. **Do NOT** create a public GitHub issue
2. **Email**: security@yourdomain.com with details
3. **Include**: Steps to reproduce, potential impact, suggested fix
4. **Wait**: Allow 90 days for patch development before disclosure

## References

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [ASP.NET Core Security](https://docs.microsoft.com/en-us/aspnet/core/security/)
- [JWT Best Practices](https://tools.ietf.org/html/rfc8725)
- [GDPR Compliance](https://gdpr-info.eu/)

---

**Last Updated**: March 8, 2024  
**Security Version**: 1.0  
**Review Frequency**: Quarterly
