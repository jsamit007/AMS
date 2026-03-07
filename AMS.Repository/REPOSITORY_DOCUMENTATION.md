# Repository Layer Documentation

## Overview

The AMS.Repository project implements the **Repository Pattern** and **Unit of Work Pattern** for data access in the Attendance Management System.

## Architecture

### Layers

```
Domain Layer (Entities)
    ↓
Repository Interfaces
    ↓
Repository Implementations
    ↓
DbContext (Entity Framework)
    ↓
SQL Server Database
```

## Components

### 1. Entity Models

Located in `Entities/`:

- **Employee.cs** - Employee master data
- **Department.cs** - Department information
- **Attendance.cs** - Daily attendance records
- **Leave.cs** - Leave requests and history

#### Entity Relationships

```
Department (1) ──→ (Many) Employee
Employee (1) ──→ (Many) Attendance
Employee (1) ──→ (Many) Leave
```

### 2. DbContext

**File**: `Context/AttendanceManagementContext.cs`

```csharp
public DbSet<Employee> Employees { get; set; }
public DbSet<Department> Departments { get; set; }
public DbSet<Attendance> Attendances { get; set; }
public DbSet<Leave> Leaves { get; set; }
```

#### Features

- Fluent API configurations
- Indexes for performance
- Foreign key constraints
- Cascade delete behaviors
- Unique constraints

### 3. Repository Interfaces

**File**: `Interfaces/IRepository.cs`

#### Generic IRepository<T>

```csharp
// Read
Task<T?> GetByIdAsync(int id);
Task<IEnumerable<T>> GetAllAsync();
Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize);
Task<int> GetTotalCountAsync();

// Create
Task<T> AddAsync(T entity);
Task AddRangeAsync(IEnumerable<T> entities);

// Update
Task<T> UpdateAsync(T entity);

// Delete
Task<bool> DeleteAsync(int id);
Task<bool> DeleteAsync(T entity);

// Persistence
Task<bool> SaveChangesAsync();
```

#### Specific Repositories

**IEmployeeRepository**
- `GetByCodeAsync(code)` - Find by employee code
- `GetByEmailAsync(email)` - Find by email
- `GetByDepartmentAsync(departmentId)` - Get employees in department
- `GetActiveEmployeesAsync()` - Get active employees only

**IAttendanceRepository**
- `GetByEmployeeAsync(employeeId)` - Get all attendance for employee
- `GetByDateRangeAsync(fromDate, toDate)` - Get attendance in date range
- `GetByEmployeeAndDateRangeAsync(employeeId, fromDate, toDate)` - Combined filter
- `GetByEmployeeAndDateAsync(employeeId, date)` - Get single date record

**IDepartmentRepository**
- `GetByCodeAsync(code)` - Find by department code
- `GetWithEmployeesAsync(id)` - Load department with all employees
- `GetActiveAsync()` - Get active departments only

**ILeaveRepository**
- `GetByEmployeeAsync(employeeId)` - Get all leaves for employee
- `GetByStatusAsync(status)` - Get leaves by status
- `GetByEmployeeAndStatusAsync(employeeId, status)` - Combined filter
- `GetByDateRangeAsync(fromDate, toDate)` - Get leaves in date range
- `GetApprovedLeaveCountByEmployeeAsync(employeeId, leaveType)` - Calculate leave balance

### 4. Repository Implementations

Located in `Repository/`:

- **Repository.cs** - Generic base repository
- **EmployeeRepository.cs** - Employee-specific queries
- **AttendanceRepository.cs** - Attendance-specific queries
- **DepartmentRepository.cs** - Department-specific queries
- **LeaveRepository.cs** - Leave-specific queries

### 5. Unit of Work Pattern

**File**: `UnitOfWork/IUnitOfWork.cs`

```csharp
public interface IUnitOfWork : IDisposable
{
    IEmployeeRepository Employees { get; }
    IAttendanceRepository Attendances { get; }
    IDepartmentRepository Departments { get; }
    ILeaveRepository Leaves { get; }

    Task<bool> SaveChangesAsync();
}
```

#### Benefits

- Single point for transaction management
- Coordinated save across multiple repositories
- Simplified dependency injection
- Consistent data changes

### 6. Dependency Injection

**File**: `Extensions/ServiceCollectionExtensions.cs`

```csharp
// In Program.cs
var connectionString = configuration.GetConnectionString("DefaultConnection");
services.AddRepositories(connectionString);
```

#### Registered Services

```
DbContext → Scoped
IUnitOfWork → Scoped
IEmployeeRepository → Scoped
IAttendanceRepository → Scoped
IDepartmentRepository → Scoped
ILeaveRepository → Scoped
```

## Usage Examples

### Using Unit of Work

```csharp
public class EmployeeService
{
    private readonly IUnitOfWork _unitOfWork;

    public EmployeeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Employee> CreateEmployeeAsync(Employee employee)
    {
        await _unitOfWork.Employees.AddAsync(employee);
        await _unitOfWork.SaveChangesAsync();
        return employee;
    }

    public async Task<Employee?> GetEmployeeByCodeAsync(string code)
    {
        return await _unitOfWork.Employees.GetByCodeAsync(code);
    }

    public async Task<IEnumerable<Employee>> GetDepartmentEmployeesAsync(int departmentId)
    {
        return await _unitOfWork.Employees.GetByDepartmentAsync(departmentId);
    }
}
```

### Using Individual Repositories

```csharp
public class AttendanceService
{
    private readonly IAttendanceRepository _attendanceRepository;

    public AttendanceService(IAttendanceRepository attendanceRepository)
    {
        _attendanceRepository = attendanceRepository;
    }

    public async Task MarkAttendanceAsync(Attendance attendance)
    {
        await _attendanceRepository.AddAsync(attendance);
    }

    public async Task<IEnumerable<Attendance>> GetEmployeeAttendanceAsync(
        int employeeId, 
        DateTime fromDate, 
        DateTime toDate)
    {
        return await _attendanceRepository.GetByEmployeeAndDateRangeAsync(
            employeeId, 
            fromDate, 
            toDate);
    }
}
```

### With Pagination

```csharp
var pageNumber = 1;
var pageSize = 10;

var employees = await _unitOfWork.Employees.GetPagedAsync(pageNumber, pageSize);
var totalCount = await _unitOfWork.Employees.GetTotalCountAsync();

var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
```

## Database Configuration

### Connection String (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=AttendanceManagementSystem;Trusted_Connection=true;Encrypt=false;"
  }
}
```

### Program.cs Setup

```csharp
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddRepositories(connectionString);

// ... other services ...

var app = builder.Build();

// Initialize database
await DataInitializer.InitializeDatabaseAsync(app.Services);

app.Run();
```

## Entity Framework Migrations

### Create Initial Migration

```bash
dotnet ef migrations add InitialCreate -p AMS.Repository -s AMS.API
```

### Update Database

```bash
dotnet ef database update -p AMS.Repository -s AMS.API
```

### View Migrations

```bash
dotnet ef migrations list -p AMS.Repository
```

### Rollback Migration

```bash
dotnet ef migrations remove -p AMS.Repository -s AMS.API
```

## Performance Considerations

### Indexes Created

- `Employee.EmployeeCode` - UNIQUE
- `Employee.Email` - UNIQUE
- `Attendance.(EmployeeId, Date)` - Composite
- `Leave.(EmployeeId, Status)` - Composite

### Lazy Loading

- Disabled by default (Explicit or Eager loading recommended)
- Use `.Include()` for related entities

### Example - Eager Loading

```csharp
var employee = await _context.Employees
    .Include(e => e.Department)
    .Include(e => e.Attendances)
    .FirstOrDefaultAsync(e => e.Id == id);
```

## Transaction Management

### Multiple Operations with Rollback

```csharp
using (var transaction = await _context.Database.BeginTransactionAsync())
{
    try
    {
        await _unitOfWork.Employees.AddAsync(employee);
        await _unitOfWork.Departments.UpdateAsync(department);
        await _unitOfWork.SaveChangesAsync();
        
        await transaction.CommitAsync();
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

## Testing

### Mock Repository Example

```csharp
var mockEmployeeRepo = new Mock<IEmployeeRepository>();
mockEmployeeRepo
    .Setup(r => r.GetByCodeAsync("EMP001"))
    .ReturnsAsync(new Employee { Id = 1, EmployeeCode = "EMP001" });

var service = new EmployeeService(mockEmployeeRepo.Object);
```

## Best Practices

1. **Always use async methods** - `GetByIdAsync()` not `GetById()`
2. **Use Unit of Work for related operations** - Transactional consistency
3. **Load required data with Include()** - Avoid N+1 queries
4. **Use Paging for large datasets** - GetPagedAsync()
5. **Validate business rules in Service layer** - Not in Repository
6. **Log repository operations** - For debugging
7. **Handle DbUpdateException** - Gracefully catch violations
8. **Use migrations for schema changes** - Never modify manually

## Common Issues & Solutions

| Issue | Solution |
|-------|----------|
| **Null reference on related entity** | Use `.Include()` in query |
| **Duplicate key exception** | Check unique constraints before insert |
| **Lazy loading not working** | Enable lazy loading or use `.Include()` |
| **Performance issues** | Add database indexes, paginate results |
| **Concurrent update conflicts** | Implement optimistic concurrency with `RowVersion` |

## File Structure

```
AMS.Repository/
├── Entities/
│   ├── Employee.cs
│   ├── Department.cs
│   ├── Attendance.cs
│   └── Leave.cs
├── Context/
│   └── AttendanceManagementContext.cs
├── Interfaces/
│   └── IRepository.cs
├── Repository/
│   ├── Repository.cs (Generic)
│   ├── EmployeeRepository.cs
│   ├── AttendanceRepository.cs
│   ├── DepartmentRepository.cs
│   └── LeaveRepository.cs
├── UnitOfWork/
│   └── IUnitOfWork.cs
├── Extensions/
│   └── ServiceCollectionExtensions.cs
└── AMS.Repository.csproj
```

## Related Projects

- **AMS.API** - Uses repositories through API controllers
- **AMS.Command** - Creates/updates data
- **AMS.Query** - Reads data
