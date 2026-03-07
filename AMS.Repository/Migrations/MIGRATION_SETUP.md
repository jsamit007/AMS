# Database Migration System - AMS

## Overview

This document describes the database migration system for the Attendance Management System, which uses **DbUp** for PostgreSQL database versioning and schema management.

## Technology Stack

- **DbUp**: Version 5.0.0 (PostgreSQL migration framework)
- **Database**: PostgreSQL (SQL)
- **Pattern**: SQL script-based migrations with automatic versioning

## Migration Architecture

### How It Works

1. **Migration Scripts**: SQL migration files stored in `AMS.Repository/Migrations/` directory
2. **DbUp Runner**: `DataInitializer.cs` class orchestrates migration execution
3. **Version Tracking**: DbUp creates a `SchemaVersions` table to track applied migrations
4. **Application Startup**: Migrations run automatically when the API application starts

### Migration Execution Flow

```
Application Start (Program.cs)
  ↓
DataInitializer.InitializeDatabaseAsync() called
  ↓
DbUp reads connection string from configuration
  ↓
DbUp scans for embedded scripts in AMS.Repository assembly
  ↓
DbUp checks SchemaVersions table for already-applied migrations
  ↓
DbUp executes new/pending migration scripts
  ↓
DbUp records script names in SchemaVersions table
  ↓
Database schema is up-to-date
```

## Current Migrations

### 001_CreateDepartmentTable.sql
- **Purpose**: Create base Department entity table
- **Creates**: 
  - `dbo.Department` table with columns: Id, Name, Code, IsActive, CreatedDate, UpdatedDate
  - Unique constraint on (Name, Code)
  - Indexes for performance optimization
- **Dependencies**: None (base table)

### 002_CreateEmployeeTable.sql
- **Purpose**: Create Employee entity table with Department relationship
- **Creates**:
  - `dbo.Employee` table with columns: Id, DepartmentId, FirstName, LastName, EmployeeCode, Email, PhoneNumber, IsActive, CreatedDate, UpdatedDate
  - Foreign key to Department table (SET NULL on delete)
  - Unique constraints on EmployeeCode and Email
  - Indexes for lookups and filters
- **Dependencies**: 001_CreateDepartmentTable.sql

### 003_CreateAttendanceTable.sql
- **Purpose**: Create Attendance entity table for tracking employee check-in/out
- **Creates**:
  - `dbo.Attendance` table with columns: Id, EmployeeId, Date, CheckInTime, CheckOutTime, Status, Remarks, CreatedDate, UpdatedDate
  - Foreign key to Employee table (CASCADE delete)
  - Unique constraint on (EmployeeId, Date)
  - Indexes on EmployeeId, Date, Status for efficient querying
- **Dependencies**: 002_CreateEmployeeTable.sql

### 004_CreateLeaveTable.sql
- **Purpose**: Create Leave request entity table
- **Creates**:
  - `dbo.Leave` table with columns: Id, EmployeeId, LeaveType, FromDate, ToDate, NumberOfDays, Reason, Status, ApprovedBy, ApprovedDate, CreatedDate, UpdatedDate
  - Foreign key to Employee table (CASCADE delete for EmployeeId, SET NULL for ApprovedBy)
  - Indexes on EmployeeId, Status, LeaveType, FromDate for efficient queries
- **Dependencies**: 002_CreateEmployeeTable.sql

## Adding New Migrations

### Step 1: Create Migration Script File
Create a new SQL file in `AMS.Repository/Migrations/` directory with naming convention:
```
NNN_DescriptiveNameOfChange.sql
```
Example: `005_AddLeaveBalanceTable.sql`

### Step 2: Write T-SQL Migration Script
```sql
-- Script: 005_AddLeaveBalanceTable
-- Description: Track annual leave balance per employee
-- Author: [Your Name]
-- Date: [Date]

CREATE TABLE [dbo].[LeaveBalance] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [EmployeeId] INT NOT NULL,
    [Year] INT NOT NULL,
    [AnnualLeaveBalance] INT NOT NULL,
    [CasualLeaveBalance] INT NOT NULL,
    [SickLeaveBalance] INT NOT NULL,
    [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedDate] DATETIME2,
    FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[Employee]([Id]) ON DELETE CASCADE,
    UNIQUE([EmployeeId], [Year])
);

CREATE INDEX IX_LeaveBalance_EmployeeId ON [dbo].[LeaveBalance]([EmployeeId]);
CREATE INDEX IX_LeaveBalance_Year ON [dbo].[LeaveBalance]([Year]);
```

### Step 3: Automatic Execution
The migration will automatically execute when the application next starts, before the API begins accepting requests.

## Best Practices

### Migration Scripts

1. **Always Be Idempotent**: Migrations should be safe to run multiple times (though DbUp won't re-run them)
   ```sql
   -- Good
   IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MyTable')
   BEGIN
       CREATE TABLE [dbo].[MyTable] (...)
   END
   
   -- Also acceptable (DbUp prevents re-runs)
   CREATE TABLE [dbo].[MyTable] (...)
   ```

2. **Include Transaction Support**: Wrap DML operations for atomicity
   ```sql
   BEGIN TRANSACTION;
   
   -- Migration logic
   
   COMMIT;
   -- Or ROLLBACK; on error
   ```

3. **Add Indexes for Common Queries**: Improve query performance
   ```sql
   CREATE INDEX IX_TableName_ColumnName ON [dbo].[TableName]([ColumnName]);
   CREATE INDEX IX_TableName_Composite ON [dbo].[TableName]([Col1], [Col2]);
   ```

4. **Use Proper Constraints**: Maintain data integrity
   ```sql
   FOREIGN KEY ([ForeignKeyCol]) REFERENCES [dbo].[ReferencedTable]([Id]) ON DELETE CASCADE
   CHECK ([Status] IN ('Active', 'Inactive'))
   UNIQUE([Email], [Code])
   ```

5. **Include Comments**: Explain the purpose of the migration
   ```sql
   -- Script: 005_AddLeaveBalanceTable
   -- Description: Track annual leave balance per employee
   -- Author: AMS Team
   -- Date: 2024-03-08
   ```

6. **Never Drop/Truncate Production Data**: Use alter/migration patterns
   ```sql
   -- Renaming a column (safe)
   EXEC sp_rename 'dbo.Employee.EmployeeCode', 'EmployeeCodeNew', 'COLUMN';
   
   -- Avoid dropping columns without backup
   -- Always backup data before destructive operations on production
   ```

## Connection String Configuration

The migration system reads the connection string from `appsettings.{Environment}.json`:

```json
"ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=ams;Username=postgres;Password=YourPassword;"
}
```

### For Different Environments

**Development** (`appsettings.Development.json`):
```json
"ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=ams_dev;Username=postgres;Password=postgres;"
}
```

**Production** (`appsettings.Production.json`):
```json
"ConnectionStrings": {
    "DefaultConnection": "Host=prod-db-server;Port=5432;Database=ams_prod;Username=postgres;Password=YourSecurePassword;"
}
```

## Troubleshooting

### Issue: Migration fails at startup
**Solution**: Check PostgreSQL connection string and database existence
```bash
# Verify connection
psql -h localhost -U postgres -d ams_dev
```

### Issue: schemaversions table doesn't exist
**Solution**: This is normal on first run - DbUp creates it automatically

### Issue: Specific migration fails but others should succeed
**Solution**: Fix the failing migration script and re-run application. DbUp tracks which migrations have been applied, so successful ones won't re-execute.

### Issue: Need to rollback a migration
**Solution**: DbUp doesn't support automatic rollbacks. You must:
1. Create a new migration script (005_RollbackXYZ.sql) with reverse operations
2. Delete the problematic migration's entry from `SchemaVersions` table if needed
3. Apply the new migration

## Monitoring Migrations

### View Applied Migrations
```sql
SELECT * FROM public.schemaversions ORDER BY applied;
```

### Check Database Schema
```sql
-- View all tables
SELECT table_name FROM information_schema.tables WHERE table_schema = 'public';

-- View table structure
SELECT column_name, data_type, is_nullable FROM information_schema.columns 
WHERE table_schema = 'public' AND table_name = 'employee';

-- View indexes
SELECT indexname FROM pg_indexes WHERE tablename = 'employee';
```

## Integration with Entity Framework Core

The migrations create SQL Server schema that EF Core maps to through the `AttendanceManagementContext`:

```csharp
public class AttendanceManagementContext : DbContext
{
    public DbSet<Department> Departments { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Attendance> Attendances { get; set; }
    public DbSet<Leave> Leaves { get; set; }
}
```

DbUp migrations and EF Core can coexist - DbUp creates/modifies schema, EF Core provides ORM access.

## Future Enhancements

Potential improvements to migration system:

1. **Seed Data Migration**: Create 005_SeedInitialData.sql for default roles, departments
2. **Migration Validation**: Add scripts to verify schema integrity post-migration
3. **Migration History Reports**: Query SchemaVersions table for deployment tracking
4. **Automated Backup**: Create pre-migration database backup logic
5. **Conditional Deployment**: Create migration prerequisites validation

## References

- [DbUp Documentation](https://dbup.readthedocs.io/)
- [SQL Server T-SQL Reference](https://learn.microsoft.com/en-us/sql/t-sql/language-reference)
- [Entity Framework Core Documentation](https://learn.microsoft.com/en-us/ef/core/)

---

**Last Updated**: 2024-03-08  
**Maintained By**: AMS Development Team
