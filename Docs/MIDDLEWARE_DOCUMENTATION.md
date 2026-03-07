# Middleware Documentation - AMS

Detailed documentation of all middleware components in the middleware pipeline.

## Middleware Pipeline Overview

Middleware components execute in order during request processing:

```
Client Request
     ↓
1. ExceptionHandlingMiddleware
2. PerformanceMiddleware
3. RequestResponseLoggingMiddleware
4. RateLimitingMiddleware
5. CORS Middleware
6. HTTPS Redirection
7. Authentication (JWT Bearer)
8. Authorization
9. ValidationErrorHandlingMiddleware
10. Routing → Controller → Action
     ↓
Response → Same middleware in reverse
     ↓
Client Response
```

Each middleware can:
- **Inspect** the request
- **Modify** request or response headers
- **Short-circuit** (return early without calling next middleware)
- **Log** information
- **Transform** data

## Middleware Components

### 1. ExceptionHandlingMiddleware

**Purpose**: Global exception handling and error transformation

**Location**: `AMS.API/Middleware/ExceptionHandlingMiddleware.cs`

**Features**:
- Catches unhandled exceptions
- Logs detailed error information
- Transforms exceptions to consistent API response format
- Returns appropriate HTTP status codes

**Configuration**:
```csharp
app.UseMiddleware<ExceptionHandlingMiddleware>();
```

**Handled Exceptions**:
- `ValidationException` → 400 Bad Request
- `NotFoundException` → 404 Not Found
- `UnauthorizedException` → 401 Unauthorized
- `ForbiddenException` → 403 Forbidden
- `Exception` (any) → 500 Internal Server Error

**Response Format**:
```json
{
  "success": false,
  "message": "An error occurred",
  "data": null,
  "errors": ["Error message"],
  "timestamp": "2024-03-08T10:30:00Z"
}
```

### 2. PerformanceMiddleware

**Purpose**: Monitor and log request/response performance metrics

**Location**: `AMS.API/Middleware/PerformanceMiddleware.cs`

**Features**:
- Measures request processing time
- Adds response header with execution duration
- Logs slow requests (>1000ms)
- Tracks database query time

**Configuration**:
```csharp
app.UseMiddleware<PerformanceMiddleware>();
```

**Response Headers**:
```
X-Response-Time-Ms: 125
```

**Logging**:
```
[Information] Request processing completed in 125ms for GET /api/employees
[Warning] Slow request detected: 1250ms for POST /api/attendance
```

**Threshold**: Warnings logged for requests exceeding 1000ms

### 3. RequestResponseLoggingMiddleware

**Purpose**: Log detailed HTTP request and response information

**Location**: `AMS.API/Middleware/RequestResponseLoggingMiddleware.cs`

**Features**:
- Logs HTTP method, path, status code
- Logs request headers (excluding sensitive headers)
- Logs request body (JSON only, limited size)
- Logs response status and headers
- Only in Development/Debug mode

**Configuration**:
```csharp
if (app.Environment.IsDevelopment())
{
    app.UseMiddleware<RequestResponseLoggingMiddleware>();
}
```

**Log Format**:
```
[Information] HTTP Request: GET /api/attendance
[Information] Headers: 
  Authorization: Bearer ***
  Content-Type: application/json
[Information] Query: pageNumber=1&pageSize=10
[Information] Response: 200 OK
```

**Excluded Headers** (not logged, sensitive data):
- Authorization
- X-API-Key
- Cookie
- Set-Cookie

**Max Body Size**: 1024 bytes (prevents logging large bodies)

### 4. RateLimitingMiddleware

**Purpose**: Prevent abuse by limiting requests per IP address

**Location**: `AMS.API/Middleware/RateLimitingMiddleware.cs`

**Features**:
- Tracks requests per IP address
- Configurable request limits per time window
- Returns 429 Too Many Requests when exceeded
- Resets counter after time window

**Configuration** (in appsettings.json):
```json
{
  "RateLimiting": {
    "MaxRequests": 100,
    "TimeWindowSeconds": 60
  }
}
```

**Environment Defaults**:
- **Development**: 200 requests per 60 seconds
- **QA**: 150 requests per 60 seconds
- **UAT/Production**: 100 requests per 60 seconds

**Response When Rate Limited** (429):
```json
{
  "success": false,
  "message": "Rate limit exceeded. Maximum 100 requests per 60 seconds.",
  "errors": []
}
```

**Response Headers**:
```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 45
X-RateLimit-Reset: 1615209600
```

**Bypass Rate Limiting** (to whitelist IPs):
```csharp
var whitelistIps = new[] { "127.0.0.1", "::1" };
if (whitelistIps.Contains(ipAddress))
{
    await next();
    return;
}
```

### 5. CORS Middleware

**Purpose**: Enable controlled cross-origin requests from web browsers

**Location**: Built-in ASP.NET Core middleware

**Configuration** (in Program.cs):
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy
            .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()!)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

app.UseCors("AllowSpecificOrigins");
```

**Environment-Specific Configuration** (appsettings.json):
```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:5173",
      "https://yourdomain.com"
    ]
  }
}
```

**Preflight Requests**:
- Browser sends OPTIONS request before actual request
- Server responds with CORS headers
- If allowed, browser sends actual request

**Response Headers**:
```
Access-Control-Allow-Origin: http://localhost:3000
Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS
Access-Control-Allow-Headers: Content-Type, Authorization
Access-Control-Allow-Credentials: true
```

### 6. Authentication Middleware (JWT Bearer)

**Purpose**: Validate JWT tokens and extract user claims

**Location**: Built-in ASP.NET Core middleware

**Configuration** (in Program.cs):
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

app.UseAuthentication();
```

**Validation Checks**:
- ✅ Token signature matches secret key
- ✅ Token hasn't expired
- ✅ Token issued by correct issuer
- ✅ Token is for correct audience
- ✅ Clock skew (1 minute tolerance)

**Token Format**:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**On Success**: User principal populated with claims

**On Failure**: Returns 401 Unauthorized

### 7. Authorization Middleware

**Purpose**: Check if authenticated user has required permissions

**Location**: Custom attribute and built-in middleware

**Policies** (configured in Program.cs):
```csharp
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", p => p.RequireRole("Admin"))
    .AddPolicy("ManagerOrAbove", p => p.RequireRole("Admin", "Manager"))
    .AddPolicy("OwnResourceOrAdmin", p => p.RequireAssertion(ctx =>
        ctx.User.IsInRole("Admin") || 
        ctx.User.FindFirst("sub")?.Value == ctx.Resource?.ToString()
    ));
```

**Usage** (on endpoints):
```csharp
[Authorize]  // Any authenticated user
public IActionResult Public() { }

[Authorize(Roles = "Admin")]  // Admin only
public IActionResult Admin() { }

[Authorize(Policy = "ManagerOrAbove")]  // Custom policy
public IActionResult Managers() { }

[AllowAnonymous]  // Bypass authorization
public IActionResult Login() { }
```

**On Failure**: Returns 403 Forbidden

### 8. ValidationErrorHandlingMiddleware

**Purpose**: Handle and format FluentValidation exceptions

**Location**: `AMS.API/Middleware/ValidationErrorHandlingMiddleware.cs`

**Features**:
- Catches `ValidationException` from FluentValidation
- Formats validation errors into response
- Returns 400 Bad Request with error details

**Response Format** (400):
```json
{
  "success": false,
  "message": "Validation failed",
  "data": null,
  "errors": [
    "First name is required",
    "Email format is invalid",
    "Employee code must be unique"
  ]
}
```

**Configuration**:
```csharp
app.UseMiddleware<ValidationErrorHandlingMiddleware>();
```

## Custom Middleware Template

Create a new middleware:

```csharp
public class CustomMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CustomMiddleware> _logger;

    public CustomMiddleware(RequestDelegate next, ILogger<CustomMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Pre-request processing
            _logger.LogInformation("Request: {Method} {Path}", 
                context.Request.Method, context.Request.Path);

            // Call next middleware
            await _next(context);

            // Post-response processing
            _logger.LogInformation("Response: {StatusCode}", 
                context.Response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in custom middleware");
            throw;
        }
    }
}

// Register in Program.cs
app.UseMiddleware<CustomMiddleware>();
```

## Middleware Ordering

**Order matters!** Middleware executes in registration order.

```csharp
// ✓ Correct order
app.UseMiddleware<ExceptionHandlingMiddleware>();  // First - catch all exceptions
app.UseMiddleware<PerformanceMiddleware>();        // Monitor all requests
app.UseMiddleware<RequestResponseLoggingMiddleware>();  // Log details
app.UseMiddleware<RateLimitingMiddleware>();       // Rate limit
app.UseCors();                                       // CORS
app.UseHttpsRedirection();                          // HTTPS
app.UseAuthentication();                            // JWT validation
app.UseAuthorization();                             // Check permissions
app.UseMiddleware<ValidationErrorHandlingMiddleware>();  // Validation errors
app.MapControllers();                               // Route to controllers
```

**Why this order?**
1. Exception handling must be first (catch everything)
2. Performance/logging capture complete flow
3. Rate limiting prevents abuse early
4. Auth before protected operations
5. Routing last (entry to controllers)

## Middleware Performance Impact

### Memory Impact
- Each middleware adds minimal overhead (~1KB)
- Total: ~10KB for all middleware combined

### Performance Impact
- Exception handling: <1ms (no exceptions)
- Performance monitoring: <1ms
- Rate limiting: <2ms (hash lookup)
- Auth: <5ms (token validation)
- Total overhead: ~10ms per request

### Optimization Tips

1. **Order by frequency**: Put most-used first
2. **Short-circuit early**: Return ASAP if possible
3. **Avoid re-reading body**:
   ```csharp
   // Enable rewind for body
   context.Request.EnableBuffering();
   // Read once
   var body = await reader.ReadAsStringAsync();
   context.Request.Body.Position = 0;  // Rewind
   ```
4. **Cache expensive operations**:
   ```csharp
   var skipLogging = context.Request.Path.StartsWithSegments("/health");
   ```

## Middleware Debugging

### Enable Detailed Logging

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Debug"
    }
  }
}
```

### Trace Middleware Execution

Add logging in middleware:

```csharp
public async Task InvokeAsync(HttpContext context)
{
    _logger.LogDebug("→ Entering middleware");
    await _next(context);
    _logger.LogDebug("← Exiting middleware");
}
```

### Monitor in Real-Time

```bash
# Watch logs during request
dotnet run | grep -i middleware
```

## Common Issues & Solutions

### Issue: Headers Already Sent
**Cause**: Middleware tries to modify response after content sent
**Solution**: Only modify headers before `await _next(context)`

### Issue: Body Can't Be Read Twice
**Cause**: Request body stream not rewound
**Solution**: Enable buffering: `context.Request.EnableBuffering()`

### Issue: Middleware Not Executing
**Cause**: Registered after routing
**Solution**: Register `app.UseMiddleware()` BEFORE `app.MapControllers()`

### Issue: Rate Limit Not Working
**Cause**: Middleware order - CORS before rate limiting
**Solution**: Register rate limiting before CORS

---

**Last Updated**: March 8, 2024  
**Middleware Version**: 1.0  
**Total Middleware Count**: 8 components
