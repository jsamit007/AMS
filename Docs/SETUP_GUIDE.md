# Setup Guide - AMS

Complete step-by-step guide to set up the Attendance Management System development environment.

## Prerequisites

### System Requirements

- **OS**: Windows 10/11, macOS 10.15+, or Ubuntu 18.04+
- **RAM**: Minimum 8GB (16GB recommended)
- **Disk Space**: 2GB for .NET SDK + development tools
- **CPU**: Multi-core processor recommended

### Required Software

1. **.NET 9.0 SDK or higher**
   - Download from: https://dotnet.microsoft.com/download
   - Verify installation: `dotnet --version`

2. **PostgreSQL 12+**
   - Download from: https://www.postgresql.org/download
   - Verify installation: `psql --version`

3. **Git**
   - Download from: https://git-scm.com/download
   - Verify installation: `git --version`

4. **IDE (Choose one)**
   - Visual Studio 2022 Community (Free)
   - Visual Studio Code with C# dev kit
   - JetBrains Rider

5. **PostgreSQL GUI (Optional)**
   - pgAdmin 4 (Web-based)
   - DBeaver (Desktop)

## Installation Steps

### 1. Clone the Repository

```bash
# Clone from GitHub
git clone https://github.com/your-org/ams.git
cd ams

# Or if using SSH key
git clone git@github.com:your-org/ams.git
cd ams
```

### 2. Setup PostgreSQL Database

#### Windows

```powershell
# Start PostgreSQL (should be running as service)
# Or open pgAdmin from Start Menu

# Open PostgreSQL command line
psql -U postgres

# In psql prompt, create database and user:
CREATE DATABASE ams_dev;
CREATE USER ams_user WITH ENCRYPTED PASSWORD 'password123';
ALTER ROLE ams_user SET client_encoding TO 'utf8';
ALTER ROLE ams_user SET default_transaction_isolation TO 'read committed';
ALTER ROLE ams_user SET default_transaction_deferrable TO on;
ALTER ROLE ams_user SET timezone TO 'UTC';
GRANT ALL PRIVILEGES ON DATABASE ams_dev TO ams_user;
\q
```

#### macOS

```bash
# Install PostgreSQL via Homebrew (if not already installed)
brew install postgresql@15

# Start PostgreSQL
brew services start postgresql@15

# Create database
createdb -U postgres ams_dev

# Create user
psql -U postgres -d ams_dev -c "CREATE USER ams_user WITH ENCRYPTED PASSWORD 'password123';"
psql -U postgres -d ams_dev -c "GRANT ALL PRIVILEGES ON DATABASE ams_dev TO ams_user;"
```

#### Linux (Ubuntu)

```bash
# Install PostgreSQL
sudo apt update
sudo apt install postgresql postgresql-contrib

# Start PostgreSQL
sudo systemctl start postgresql

# Switch to postgres user
sudo -u postgres psql

# In psql prompt:
CREATE DATABASE ams_dev;
CREATE USER ams_user WITH ENCRYPTED PASSWORD 'password123';
ALTER ROLE ams_user SET client_encoding TO 'utf8';
ALTER ROLE ams_user SET default_transaction_isolation TO 'read committed';
ALTER ROLE ams_user SET default_transaction_deferrable TO on;
ALTER ROLE ams_user SET timezone TO 'UTC';
GRANT ALL PRIVILEGES ON DATABASE ams_dev TO ams_user;
\q
```

### 3. Configure Connection String

Edit `AMS.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=ams_dev;Username=ams_user;Password=password123;"
  }
}
```

**Note**: Never commit sensitive credentials. Use environment variables or secrets management in production.

### 4. Restore NuGet Packages

```bash
cd ams
dotnet restore
```

This downloads all required NuGet packages specified in `.csproj` files.

### 5. Build the Solution

```bash
dotnet build
```

Verify there are no compilation errors.

### 6. Run Database Migrations

Migrations run automatically on application startup, but you can also run them manually:

```bash
cd AMS.API
dotnet run
```

Watch the console for migration execution messages:
```
Database migration completed successfully.
```

### 7. Verify Setup

#### Test Database Connection

```bash
# Open PostgreSQL client
psql -U ams_user -d ams_dev -h localhost

# List tables
\dt

# Verify tables exist:
# - department
# - employee
# - attendance
# - leave
# - schemaversions

\q
```

#### Test API

The API should be running on:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:7001`

Open Swagger UI: `https://localhost:7001/swagger`

You should see the API schema with all endpoints listed.

## Authentication Setup

### JWT Configuration

Edit `AMS.API/appsettings.{Environment}.json` and add/configure the Authentication section:

```json
{
  "Authentication": {
    "Jwt": {
      "SecretKey": "your-super-secret-key-minimum-32-characters-long-e.g.-prod-use-strong-random-string",
      "Issuer": "ams-api",
      "Audience": "ams-users",
      "AccessTokenExpirationMinutes": 15,
      "RefreshTokenExpirationDays": 7,
      "RequireHttpsMetadata": false,
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

### Initialize Default Roles (Optional)

Create a data seed class or manually create roles in the database:

**Default System Roles**:
- **Admin** - Full system access
- **Manager** - Department and team management  
- **User** - Basic employee access

**Using Identity API** (add to `Program.cs` or startup code):

```csharp
// Seed default roles  
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
    
    string[] roleNames = { "Admin", "Manager", "User" };
    
    foreach (var roleName in roleNames)
    {
        var roleExist = await roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            await roleManager.CreateAsync(new AppRole { Name = roleName });
        }
    }
}
```

### Test Authentication

1. **Register a user** (via API endpoint):
   ```
   POST /api/auth/register
   ```

2. **Login** to get tokens:
   ```
   POST /api/auth/login
   ```

3. **Use access token** in protected endpoints:
   ```
   Authorization: Bearer <access-token>
   ```

For detailed authentication examples, see [AMS.Authentication/README.md](../AMS.Authentication/README.md).

## Development Setup

### IDE Configuration

#### Visual Studio 2022

1. Open solution: `AMS.sln`
2. Right-click `AMS.API` → Set as Startup Project
3. Press F5 to run with debugger

#### Visual Studio Code

```bash
# Install C# Dev Kit extension from marketplace

# Open workspace
code .

# Run with debugger
Press F5
```

#### JetBrains Rider

1. Open project: `AMS` folder
2. Select `AMS.API` as run configuration
3. Press Shift+F10 to run

### Create Environment File (Optional)

Create `.env` file in project root (don't commit to Git):

```bash
# Database
DB_HOST=localhost
DB_PORT=5432
DB_NAME=ams_dev
DB_USER=ams_user
DB_PASSWORD=password123

# JWT
JWT_SECRET_KEY=your-super-secret-key-minimum-32-characters-long
JWT_ISSUER=ams-issuer-dev
JWT_AUDIENCE=ams-audience-dev

# AWS (if using CloudWatch)
AWS_ACCESS_KEY_ID=your-access-key
AWS_SECRET_ACCESS_KEY=your-secret-key
AWS_REGION=us-east-1
```

Load from `Program.cs` if using environment variables.

## Common Issues & Solutions

### Issue: PostgreSQL Connection Failed

**Error**: `Host not found`

**Solution**:
```bash
# Check if PostgreSQL is running
psql -U postgres  # Should connect successfully

# Edit connection string in appsettings.Development.json
# Verify Host, Port, Database, Username, Password
```

### Issue: Database Already Exists

**Error**: `Database ams_dev already exists`

**Solution**:
```bash
# Drop existing database
psql -U postgres -c "DROP DATABASE IF EXISTS ams_dev;"

# Recreate
psql -U postgres -c "CREATE DATABASE ams_dev;"
```

### Issue: Port Already in Use

**Error**: `Address already in use (port 5432 or 7001)`

**Solution**:
```bash
# Find process using port
# Windows
netstat -ano | findstr :7001

# Linux/Mac
lsof -i :7001

# Kill the process or use different port in launchSettings.json
```

### Issue: NuGet Restore Fails

**Error**: `Unable to find assembly` or package errors

**Solution**:
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore again
dotnet restore
```

### Issue: Build Fails with Entity Framework

**Error**: `The entity type cannot be mapped to...`

**Solution**:
```bash
# Ensure DbContext is properly configured
# Check Configuration folder in AMS.Repository

# Rebuild solution
dotnet clean
dotnet build
```

## Verification Checklist

- [ ] .NET 9.0 SDK installed (`dotnet --version`)
- [ ] PostgreSQL running (`psql -U postgres`)
- [ ] Database `ams_dev` created
- [ ] User `ams_user` created with correct permissions
- [ ] Connection string configured in `appsettings.Development.json`
- [ ] NuGet packages restored (`dotnet restore`)
- [ ] Solution builds successfully (`dotnet build`)
- [ ] Migrations executed (check `schemaversions` table)
- [ ] API runs without errors (`dotnet run` from AMS.API)
- [ ] Swagger UI accessible at `https://localhost:7001/swagger`
- [ ] Database tables created (department, employee, attendance, leave)

## Next Steps

1. **Review Architecture**: See [ARCHITECTURE.md](ARCHITECTURE.md)
2. **Explore API**: Visit Swagger UI at https://localhost:7001/swagger
3. **Read Database Schema**: See [DATABASE.md](DATABASE.md)
4. **Setup Authentication**: Configure JWT tokens (see [SECURITY.md](SECURITY.md))
5. **Write Your First API Call**: Use Swagger to test endpoints
6. **Setup IDE Debugging**: Set breakpoints and debug

## Database Backup

Regularly backup your development database:

```bash
# Backup
pg_dump -U ams_user -d ams_dev > ams_backup.sql

# Restore
psql -U ams_user -d ams_dev < ams_backup.sql
```

## Performance Tips

1. **Enable Query Logging**: Add to `appsettings.Development.json`
   ```json
   "Logging": {
     "LogLevel": {
       "Microsoft.EntityFrameworkCore.Database.Command": "Information"
     }
   }
   ```

2. **Use Connection Pooling**: Configured by default in Npgsql
   ```
   "Connection Lifetime=5"  // in connection string
   ```

3. **Monitor Database**: Use pgAdmin or command line
   ```bash
   psql -U ams_user -d ams_dev -c "SELECT * FROM pg_stat_statements;"
   ```

## Uninstall/Cleanup

To remove the development environment:

```bash
# Drop database
psql -U postgres -c "DROP DATABASE IF EXISTS ams_dev;"

# Remove user (optionally)
psql -U postgres -c "DROP USER IF EXISTS ams_user;"

# Remove .NET workloads
dotnet workload remove aspnetcore

# Clean build artifacts
dotnet clean
rm -rf bin/ obj/

# Remove cache
rm -rf ~/.nuget/packages  # or %USERPROFILE%\.nuget\packages on Windows
```

## Support

For setup issues:
1. Check PostgreSQL logs: `pg_log` directory
2. Review application logs: `logs/` folder
3. Browse [TROUBLESHOOTING.md](TROUBLESHOOTING.md)
4. Check .NET diagnostics: `dotnet --info`

---

**Last Updated**: March 8, 2024  
**Compatible With**: .NET 9.0, PostgreSQL 12+
