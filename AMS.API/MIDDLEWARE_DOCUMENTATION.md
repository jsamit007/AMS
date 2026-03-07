# AMS API Middleware Documentation

## Overview
This document describes all the middleware components added to the Attendance Management System (AMS) API.

## Middleware Components

### 1. Exception Handling Middleware
**File**: `ExceptionHandlingMiddleware.cs`

Catches all unhandled exceptions and returns a standardized error response.

**Features**:
- Logs all exceptions
- Returns appropriate HTTP status codes
- Provides consistent error response format
- Handles specific exception types (ArgumentNullException, KeyNotFoundException, etc.)

**Usage**: Automatically registered in the middleware pipeline

---

### 2. Request/Response Logging Middleware
**File**: `RequestResponseLoggingMiddleware.cs`

Logs all HTTP requests and responses with timing information.

**Features**:
- Logs HTTP method, path, and status code
- Measures response time in milliseconds
- Non-intrusive response body copying

**Usage**: Automatically registered in the middleware pipeline

---

### 3. Validation Error Handling Middleware
**File**: `ValidationErrorHandlingMiddleware.cs`

Handles FluentValidation exceptions and model validation errors.

**Features**:
- Catches validation exceptions
- Groups errors by property
- Returns detailed validation error messages
- Returns 400 Bad Request status

**Requires**: FluentValidation NuGet package

**Usage**: Automatically registered in the middleware pipeline

---

### 4. CORS Policy Middleware
**File**: `CorsPolicyMiddleware.cs`

Enables Cross-Origin Resource Sharing (CORS) for frontend applications.

**Features**:
- Configurable allowed origins from appsettings.json
- Allows credentials
- Exposes custom headers
- Supports all HTTP methods

**Configuration** (appsettings.json):
```json
"Cors": {
  "AllowedOrigins": [
    "http://localhost:3000",
    "http://localhost:5173"
  ]
}
```

**Usage**: Registered via `MiddlewareExtensions.AddApiServices()`

---

### 5. Authentication Middleware
**File**: `AuthenticationMiddleware.cs`

Implements JWT Bearer token authentication.

**Features**:
- JWT token validation
- Configurable issuer and audience
- Token expiration validation
- Handles authentication failures

**Configuration** (appsettings.json):
```json
"Jwt": {
  "SecretKey": "your-super-secret-key-minimum-32-characters",
  "Issuer": "ams-issuer",
  "Audience": "ams-audience",
  "ExpirationMinutes": 60
}
```

**Usage**: Registered via `MiddlewareExtensions.AddApiServices()`

**Protected Endpoints**: Add `[Authorize]` attribute to controllers/actions

---

### 6. Authorization Middleware
**File**: `AuthorizationMiddleware.cs`

Defines role-based authorization policies.

**Available Policies**:
- `AdminOnly` - Admin role only
- `ManagerOnly` - Manager or Admin
- `EmployeeOnly` - Employee, Manager, or Admin
- `ReportAccess` - Manager, Admin, or HR
- `LeaveApproval` - Manager, Admin, or HR

**Usage in Controllers**:
```csharp
[Authorize(Policy = "ManagerOnly")]
public async Task<IActionResult> ApproveLeave(int id)
{
    // Implementation
}
```

---

### 7. Content Negotiation Middleware
**File**: `ContentNegotiationMiddleware.cs`

Configures content type handling and JSON serialization.

**Features**:
- Respects browser Accept headers
- Returns 406 Not Acceptable for unsupported content types
- CamelCase JSON property names
- Formatted JSON output (indented)

**Supported Content Types**:
- application/json
- application/xml

---

### 8. Rate Limiting Middleware
**File**: `RateLimitingMiddleware.cs`

Prevents API abuse by limiting requests per client.

**Features**:
- IP-based rate limiting
- Configurable request limits and time windows
- Default: 100 requests per 60 seconds
- Returns 429 Too Many Requests when exceeded

**Configuration**:
```csharp
// In Program.cs
app.UseMiddleware<RateLimitingMiddleware>(maxRequests: 100, timeWindowSeconds: 60);
```

---

## Setup Instructions

### 1. Update Program.cs

```csharp
using AMS.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add API services
builder.Services.AddApiServices(builder.Configuration);

// Add Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AMS API", Version = "v1" });
});

var app = builder.Build();

// Use API middleware
app.UseApiMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
```

### 2. Update appsettings.json

```json
{
  "Jwt": {
    "SecretKey": "your-secret-key-here",
    "Issuer": "ams-issuer",
    "Audience": "ams-audience"
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "https://your-frontend-domain.com"
    ]
  }
}
```

### 3. Install Required NuGet Packages

```bash
# JWT Bearer for authentication
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer

# FluentValidation for validation
dotnet add package FluentValidation

# Swagger/OpenAPI
dotnet add package Swashbuckle.AspNetCore

# API Versioning
dotnet add package Microsoft.AspNetCore.Mvc.Versioning
```

---

## Middleware Execution Order

The middleware executes in the following order:

1. **Exception Handling** - Catches all exceptions
2. **Request/Response Logging** - Logs requests and responses
3. **Rate Limiting** - Prevents abuse
4. **HTTPS Redirection** - Enforces HTTPS
5. **CORS** - Handles cross-origin requests
6. **Authentication** - Validates JWT tokens
7. **Authorization** - Checks user roles and policies
8. **Validation Error Handling** - Handles validation failures
9. **Routing** - Routes to appropriate controller

---

## Example: Protecting an Endpoint

```csharp
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
public class LeaveController : ControllerBase
{
    [Authorize(Policy = "LeaveApproval")]
    [HttpPut("{id}/approve")]
    public async Task<IActionResult> ApproveLeave(int id)
    {
        // Only Manager, Admin, or HR can access
    }

    [Authorize(Policy = "EmployeeOnly")]
    [HttpPost]
    public async Task<IActionResult> ApplyLeave(CreateLeaveDto dto)
    {
        // All authenticated users can access
    }

    [AllowAnonymous]
    [HttpGet("public-info")]
    public async Task<IActionResult> GetPublicInfo()
    {
        // No authentication required
    }
}
```

---

## Error Response Format

All errors follow this format:

```json
{
  "success": false,
  "message": "Error description",
  "data": null,
  "errors": [
    "Specific error 1",
    "Specific error 2"
  ]
}
```

---

## Logging

All middleware activities are logged using ILogger. Configure logging levels in appsettings.json:

```json
"Logging": {
  "LogLevel": {
    "Default": "Information",
    "Microsoft.AspNetCore": "Warning",
    "AMS.API.Middleware": "Debug"
  }
}
```

---

## Security Best Practices

1. **JWT Secret**: Change the secret key in production to a strong, random value (minimum 32 characters)
2. **CORS**: Configure allowed origins carefully - never use wildcard (*) in production
3. **Rate Limiting**: Adjust limits based on your API's expected usage patterns
4. **HTTPS**: Always use HTTPS in production
5. **Token Expiration**: Configure appropriate JWT expiration times
6. **Logging**: Monitor logs for suspicious activity

---

## Testing

### Testing Authentication

```bash
# Get JWT token (implement token endpoint first)
curl -X POST https://localhost:7001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"user","password":"pass"}'

# Use token in request
curl -X GET https://localhost:7001/api/employees \
  -H "Authorization: Bearer <token>"
```

### Testing CORS

```bash
curl -X OPTIONS https://localhost:7001/api/employees \
  -H "Origin: http://localhost:3000" \
  -H "Access-Control-Request-Method: GET"
```

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| 401 Unauthorized on protected endpoints | Ensure valid JWT token is provided in Authorization header |
| 403 Forbidden | User role doesn't match policy requirements |
| 429 Too Many Requests | Wait before making more requests or increase rate limit |
| CORS errors | Check allowed origins in appsettings.json |
| 400 Bad Request | Review validation errors in response for details |

