# Troubleshooting Guide - AMS

Solutions to common issues and problems encountered during development and deployment.

## Database Connection Issues

### Problem: Connection String Format Invalid

**Symptoms**:
```
System.ArgumentException: Connection string is not valid
```

**Solution**:
Verify connection string format:

```json
// PostgreSQL Format (CORRECT)
"Host=localhost;Port=5432;Database=ams_dev;Username=postgres;Password=postgres;"

// SQL Server Format (INCORRECT for PostgreSQL)
"Server=.;Database=ams_dev;Trusted_Connection=true;"
```

### Problem: PostgreSQL Connection Refused

**Symptoms**:
```
cannot connect to server: No address associated with hostname
```

**Steps**:
1. Verify PostgreSQL is running
   ```bash
   psql -U postgres -c "\l"  # List databases
   ```

2. Check connection parameters
   ```bash
   psql -h localhost -p 5432 -U postgres -d postgres
   ```

3. Check firewall (if remote)
   ```bash
   telnet [host] 5432
   ```

4. Verify credentials in appsettings
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Port=5432;Database=ams_dev;Username=YOUR_USER;Password=YOUR_PASSWORD;"
   }
   ```

### Problem: Database Does Not Exist

**Symptoms**:
```
FATAL: database "ams_dev" does not exist
```

**Solution**:
Create the database:

```bash
psql -U postgres -c "CREATE DATABASE ams_dev;"
```

Or use pgAdmin GUI to create database.

### Problem: Permission Denied on Database

**Symptoms**:
```
ERROR: permission denied for database ams_dev
```

**Solution**:
Grant privileges to user:

```bash
psql -U postgres -d ams_dev -c "GRANT ALL PRIVILEGES ON DATABASE ams_dev TO ams_user;"
```

## Entity Framework Core Issues

### Problem: Pending Migrations

**Symptoms**:
```
Microsoft.EntityFrameworkCore.Design.OperationException: Unable to create an object of type 'AttendanceManagementContext'.
```

**Solution**:
Ensure DbContext is properly configured with connection string:

```csharp
// Program.cs
builder.Services.AddDbContext<AttendanceManagementContext>(options =>
    options.UseNpgsql(connectionString));
```

### Problem: Model Property Not Mapped

**Symptoms**:
```
The entity type 'Employee' requires a primary key to be defined
```

**Solution**:
Ensure entity has `Id` property:

```csharp
public class Employee
{
    public int Id { get; set; }  // Primary key required
    public string FirstName { get; set; }
    // other properties
}
```

### Problem: Navigation Property Circular Reference

**Symptoms**:
```
JSON serialization error: Circular dependency detected
```

**Solution**:
Use DTOs instead of entities in API responses:

```csharp
// ❌ Bad - returns entity with navigation properties
public async Task<Employee> GetEmployee(int id)
{
    return await context.Employees.FirstAsync();  // Circular refs
}

// ✅ Good - returns DTO
public async Task<EmployeeDto> GetEmployee(int id)
{
    var employee = await context.Employees.FirstAsync();
    return new EmployeeDto { Id = employee.Id, Name = employee.Name };
}
```

## Migration Issues

### Problem: Migration Fails at Startup

**Symptoms**:
```
Database migration failed: An error occurred while updating the database
```

**Steps**:
1. Check migration script for errors
   ```bash
   # View migration files
   ls AMS.Repository/Migrations/*.sql
   ```

2. Check PostgreSQL logs
   ```bash
   tail -f /var/log/postgresql/postgresql.log  # Linux
   ```

3. Run migration manually to see error
   ```bash
   psql -U postgres -d ams_dev -f AMS.Repository/Migrations/001_CreateDepartmentTable.sql
   ```

4. Fix script syntax and redeploy

### Problem: Column Already Exists

**Symptoms**:
```
PostgreSQL error: column "employee_id" of relation "attendance" already exists
```

**Solution**:
Migration script needs to be idempotent:

```sql
-- ❌ Bad
ALTER TABLE attendance ADD COLUMN employee_id INTEGER;

-- ✅ Good
ALTER TABLE attendance ADD COLUMN IF NOT EXISTS employee_id INTEGER;
```

### Problem: Cannot Drop Table Due to Foreign Keys

**Symptoms**:
```
ERROR: cannot drop table employee because other objects depend on it
```

**Solution**:
Drop dependent tables first or use CASCADE:

```sql
-- Option 1: Drop with cascade
DROP TABLE employee CASCADE;

-- Option 2: Drop foreign key first
ALTER TABLE attendance DROP CONSTRAINT fk_attendance_employee;
DROP TABLE employee;
```

## Authentication Issues

### Problem: JWT Token Validation Fails

**Symptoms**:
```
The token was invalid: the token did not have a kid header parameter
```

**Solution**:
1. Verify token creation includes all required claims
   ```csharp
   var claims = new List<Claim>
   {
       new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
       new Claim(ClaimTypes.Email, email),
       new Claim(ClaimTypes.Role, role)
   };
   ```

2. Verify token signing is correct
   ```csharp
   var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
   var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
   ```

### Problem: 401 Unauthorized on Valid Token

**Symptoms**:
```
StatusCode: 401, ReasonPhrase: 'Unauthorized'
```

**Debugging**:
1. Check token format in header
   ```bash
   curl -H "Authorization: Bearer YOUR_TOKEN" http://localhost:5000/api/test
   ```

2. Verify token hasn't expired
   ```csharp
   // Decode token to check expiration
   var handler = new JwtSecurityTokenHandler();
   var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
   var expirationDate = jwtToken.ValidTo;
   ```

3. Check token secret key matches
   ```bash
   # Ensure appsettings JWT secret matches token creation secret
   ```

### Problem: 403 Forbidden Despite Valid Token

**Symptoms**:
```
StatusCode: 403, ReasonPhrase: 'Forbidden'
```

**Solution**:
1. Verify user has required role
   ```csharp
   var roles = User.FindAll(ClaimTypes.Role);
   ```

2. Check [Authorize] attribute on endpoint
   ```csharp
   [Authorize(Roles = "Admin")]  // Check specified roles
   public IActionResult AdminOnly() { }
   ```

3. Verify policy configuration
   ```csharp
   // Ensure policy is defined and user meets requirements
   ```

## API Request Issues

### Problem: 400 Bad Request

**Symptoms**:
```json
{
  "success": false,
  "message": "Validation failed",
  "errors": ["First name is required"]
}
```

**Solution**:
1. Review validation error message
2. Fix request data
3. Resend request

**Example Fix**:
```bash
# ❌ Bad request
POST /api/employees
{
  "firstName": ""  # Required field empty
}

# ✅ Good request
POST /api/employees
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john@example.com"
}
```

### Problem: 404 Not Found

**Symptoms**:
```
StatusCode: 404, ReasonPhrase: 'Not Found'
```

**Debugging**:
1. Verify resource exists
   ```bash
   GET /api/employees/999  # Wrong ID
   ```

2. Check endpoint spelling
   ```bash
   # ❌ Wrong
   GET /api/employes  # Typo
   
   # ✅ Correct
   GET /api/employees
   ```

3. Verify resource was created
   ```sql
   SELECT * FROM employee WHERE id = 999;  -- Check database
   ```

### Problem: 429 Too Many Requests

**Symptoms**:
```
StatusCode: 429, ReasonPhrase: 'Too Many Requests'
```

**Solution**:
1. Reduce request rate
2. Wait for rate limit window (check `X-RateLimit-Reset` header)
3. Request whitelist if needed

**Development workaround**:
Disable rate limiting temporarily:
```json
{
  "RateLimiting": {
    "MaxRequests": 10000,
    "TimeWindowSeconds": 60
  }
}
```

### Problem: 500 Internal Server Error

**Symptoms**:
```
StatusCode: 500, ReasonPhrase: 'Internal Server Error'
```

**Debugging**:
1. Check application logs
   ```bash
   # View last 50 errors
   cat logs/ams-app.log | grep -i error | tail -50
   ```

2. Enable detailed error messages (Development only)
   ```csharp
   if (app.Environment.IsDevelopment())
   {
       app.UseDeveloperExceptionPage();
   }
   ```

3. Check CloudWatch logs (Production)
   ```bash
   aws logs tail /ams/api/production --follow
   ```

## Performance Issues

### Problem: Slow API Responses

**Symptoms**:
```
[Warning] Request processing completed in 5000ms
```

**Debugging**:
1. Identify slow operations
   ```csharp
   var sw = Stopwatch.StartNew();
   var results = await GetDataAsync();
   sw.Stop();
   _logger.LogWarning("Query took {Milliseconds}ms", sw.ElapsedMilliseconds);
   ```

2. Check database query performance
   ```sql
   EXPLAIN ANALYZE SELECT * FROM employee WHERE department_id = 1;
   ```

3. Add missing indexes
   ```sql
   CREATE INDEX idx_employee_department ON employee(department_id);
   ```

4. Fix N+1 query problems
   ```csharp
   // ❌ Bad - N+1 queries
   var emps = context.Employees.ToList();
   foreach(var emp in emps) { var att = emp.Attendances.ToList(); }
   
   // ✅ Good - 1 query with include
   var emps = context.Employees.Include(e => e.Attendances).ToList();
   ```

### Problem: High Memory Usage

**Symptoms**:
```
Process: ams-api.dll, Memory: 1.2 GB (high!)
```

**Solution**:
1. Check for memory leaks
   ```csharp
   // ❌ Bad - creates large list in memory
   var allRecords = context.Attendances.ToList();  // 1M records
   
   // ✅ Good - process in batches
   const int batchSize = 100;
   var processed = 0;
   while(processed < total)
   {
       var batch = context.Attendances
           .Skip(processed)
           .Take(batchSize)
           .ToList();
       ProcessBatch(batch);
       processed += batchSize;
   }
   ```

2. Dispose resources properly
   ```csharp
   // ✅ Good
   using (var context = new DbContext())
   {
       // Use context
   } // Automatically disposed
   ```

## Logging Issues

### Problem: No Logs Appearing

**Symptoms**:
```
No logs generated, silent operation
```

**Solution**:
1. Check LogLevel in appsettings
   ```json
   {
     "Logging": {
       "LogLevel": {
         "Default": "Information"  // Not "None"
       }
     }
   }
   ```

2. Restart application
   ```bash
   dotnet run
   ```

### Problem: Logs Not Writing to File

**Symptoms**:
```
logs/ directory empty or doesn't exist
```

**Solution**:
1. Create logs directory
   ```bash
   mkdir logs
   chmod 644 logs  # Linux
   ```

2. Configure file sink in Serilog
   ```csharp
   .WriteTo.File(
       "logs/ams-app.log",
       rollingInterval: RollingInterval.Daily,
       retainedFileCountLimit: 10
   )
   ```

## Development Issues

### Problem: NuGet Package Not Found

**Symptoms**:
```
NU1101: Unable to find package
```

**Solution**:
1. Restore packages
   ```bash
   dotnet restore
   ```

2. Clear NuGet cache
   ```bash
   dotnet nuget locals all --clear
   ```

3. Check package version is correct
   ```bash
   dotnet package update --check-outdated
   ```

### Problem: Visual Studio Not Finding C# Project

**Symptoms**:
```
Unable to load the project
```

**Solution**:
1. Close Visual Studio
2. Delete `.vs` folder
   ```bash
   rm -rf .vs
   ```
3. Reload solution

## Common Error Messages

| Error | Cause | Solution |
|-------|-------|----------|
| `connection refused` | Database not running | Start PostgreSQL |
| `invalid column name` | Wrong column name in query | Check schema |
| `unique constraint violation` | Duplicate value | Use unique ID/email |
| `foreign key constraint` | Referenced record doesn't exist | Create parent first |
| `cannot execute query in read mode` | On read-only replica | Use primary database |
| `pool timeout` | Connection pool exhausted | Increase pool size |
| `locked by another process` | Database locked | Restart database |

## Getting Help

1. **Check Documentation**
   - Docs/ folder (README, ARCHITECTURE, API_DOCUMENTATION)
   - GitHub wiki

2. **Search Logs**
   ```bash
   grep -r "error message" logs/
   ```

3. **Enable Debug Logging**
   ```json
   {
     "Logging": {
       "LogLevel": {
         "Default": "Debug"
       }
     }
   }
   ```

4. **Consult Team**
   - Slack #ams-support
   - Email: support@yourdomain.com
   - Issue tracker on GitHub

---

**Last Updated**: March 8, 2024  
**Common Issues Resolved**: 30+
