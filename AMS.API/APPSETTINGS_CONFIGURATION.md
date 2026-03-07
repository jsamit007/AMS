# AppSettings Configuration Guide

## Overview
This guide explains how to use environment-specific appsettings files in the Attendance Management System API.

## Environment Files

### Files Created

1. **appsettings.json** - Base configuration (fallback for all environments)
2. **appsettings.Development.json** - Development environment
3. **appsettings.QA.json** - Quality Assurance environment
4. **appsettings.UAT.json** - User Acceptance Testing environment
5. **appsettings.Production.json** - Production environment

## Environment-Specific Configuration

### Development (appsettings.Development.json)
- **Database**: Local SQL Server (`.`)
- **Logging Level**: Information
- **Swagger**: Enabled
- **CORS Origins**: localhost:3000, localhost:5173, localhost:4200
- **Rate Limiting**: 200 requests/60 seconds
- **JWT Settings**: Shorter expiration (60 minutes)

### QA (appsettings.QA.json)
- **Database**: QA server database
- **Logging Level**: Information
- **Swagger**: Enabled
- **CORS Origins**: qa-ams.yourdomain.com, qa-frontend.yourdomain.com
- **Rate Limiting**: 150 requests/60 seconds
- **JWT Settings**: Medium expiration (120 minutes)

### UAT (appsettings.UAT.json)
- **Database**: UAT server database
- **Logging Level**: Warning (reduced verbosity)
- **Swagger**: Disabled
- **CORS Origins**: uat-ams.yourdomain.com, uat-frontend.yourdomain.com
- **Rate Limiting**: 100 requests/60 seconds
- **JWT Settings**: Longer expiration (180 minutes)

### Production (appsettings.Production.json)
- **Database**: Production database
- **Logging Level**: Error only
- **Swagger**: Disabled
- **CORS Origins**: ams.yourdomain.com, frontend.yourdomain.com
- **Rate Limiting**: 100 requests/60 seconds
- **JWT Settings**: Longest expiration (240 minutes)

## Setting the Environment

### Windows (Command Prompt)
```batch
set ASPNETCORE_ENVIRONMENT=Development
dotnet run
```

### Windows (PowerShell)
```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run
```

### Linux/macOS (Bash)
```bash
export ASPNETCORE_ENVIRONMENT=Development
dotnet run
```

### IIS (web.config)
```xml
<system.webServer>
  <aspNetCore>
    <environmentVariables>
      <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="QA" />
    </environmentVariables>
  </aspNetCore>
</system.webServer>
```

### Docker
```dockerfile
ENV ASPNETCORE_ENVIRONMENT=Production
```

### Docker Compose
```yaml
services:
  ams-api:
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
```

### Azure App Service
Set in Application Settings:
- Key: `ASPNETCORE_ENVIRONMENT`
- Value: `Development` or `QA` or `UAT` or `Production`

## How It Works

ASP.NET Core loads configuration files in this order:

1. **appsettings.json** - Loaded first (base configuration)
2. **appsettings.{ENVIRONMENT}.json** - Loaded second (overwrites base)
3. **Environment Variables** - Loaded third (highest priority)
4. **User Secrets** - Loaded (development only)
5. **Command-line Arguments** - Loaded (highest priority)

### Example Resolution
If `ASPNETCORE_ENVIRONMENT=Development`:
1. `appsettings.json` is loaded
2. `appsettings.Development.json` is loaded and merges/overwrites
3. Result: Development-specific config with Development values

## Program.cs Configuration Example

```csharp
var builder = WebApplication.CreateBuilder(args);

// Configuration is automatically loaded based on ASPNETCORE_ENVIRONMENT
// builder.Configuration contains merged configuration from:
// - appsettings.json
// - appsettings.{Environment}.json
// - Environment variables
// - User secrets (dev only)

var app = builder.Build();

// You can access environment name
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Environment.IsProduction())
{
    // Production-specific code
}

// Or use custom environment names
if (app.Environment.IsEnvironment("QA"))
{
    // QA-specific code
}
```

## Configuration Access in Code

```csharp
// Inject IConfiguration
public class YourService
{
    private readonly IConfiguration _configuration;

    public YourService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void UseSettings()
    {
        // Access nested values
        string secretKey = _configuration["Jwt:SecretKey"];
        string connectionString = _configuration["Database:ConnectionString"];
        
        // Get section
        var jwtSettings = _configuration.GetSection("Jwt");
        var expirationMinutes = jwtSettings.GetValue<int>("ExpirationMinutes");
    }
}

// Or use Options pattern
services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
```

## Important Security Notes

⚠️ **SECRETS MANAGEMENT**

Never commit sensitive information to version control:
- Database passwords
- JWT secret keys
- API keys
- Connection strings with credentials

### Use User Secrets (Development Only)
```bash
dotnet user-secrets init
dotnet user-secrets set "Jwt:SecretKey" "your-secret-key-here"
```

### Use Azure Key Vault (Production)
```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri("https://yourvault.vault.azure.net/"),
    new DefaultAzureCredential());
```

### Use Environment Variables (Production)
```bash
export Jwt__SecretKey="your-secret-key-here"
export Database__ConnectionString="server=...password=..."
```

## Configuration Key Naming

Note the naming convention:
- **JSON**: Nested with colons (`Jwt:SecretKey`)
- **Environment Variables**: Use double underscores (`Jwt__SecretKey`)

## Validation Checklist

Before deploying to each environment:

- [ ] Database connection string is correct
- [ ] JWT secret key is strong (32+ characters)
- [ ] CORS allowed origins are correct
- [ ] Logging levels are appropriate
- [ ] Rate limiting is configured
- [ ] Swagger is disabled (if needed)
- [ ] API base URL is correct
- [ ] All sensitive values are externalized

## Testing Configuration

```bash
# Run with Development environment
ASPNETCORE_ENVIRONMENT=Development dotnet run

# Run with QA environment
ASPNETCORE_ENVIRONMENT=QA dotnet run

# Run with UAT environment
ASPNETCORE_ENVIRONMENT=UAT dotnet run

# Run with Production environment
ASPNETCORE_ENVIRONMENT=Production dotnet run
```

## Troubleshooting

### Issue: Configuration not loading
- Check environment variable spelling (case-sensitive on Linux)
- Verify file names match exact casing
- Ensure JSON is valid

### Issue: Old settings being used
- Clear cache: `dotnet clean`
- Stop any running processes
- Verify ASPNETCORE_ENVIRONMENT is set correctly

### Issue: Secrets exposed
- Remove from git history immediately
- Rotate credentials
- Use 'dotnet user-secrets remove' to clean local secrets
- Update configuration sources

## Best Practices

1. **Keep appsettings.json generic** - Use it as a template
2. **Override in environment files** - Only override what differs
3. **Use strong secrets** - Minimum 32 characters for production
4. **Document each setting** - Add comments in files
5. **Test all environments** - Before production deployment
6. **Rotate secrets regularly** - Especially JWT keys
7. **Monitor logging** - Adjust levels per environment
8. **Use secrets management** - Never hardcode sensitive data

## Related Files

- See [MIDDLEWARE_DOCUMENTATION.md](MIDDLEWARE_DOCUMENTATION.md) for authentication and authorization setup
- See [API_DOCUMENTATION.md](API_DOCUMENTATION.md) for API endpoint specifications
