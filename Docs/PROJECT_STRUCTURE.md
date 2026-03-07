# Project Structure - AMS

Detailed breakdown of the project folder and file organization.

## Solution Overview

```
AMS/                                    # Root solution folder
├── AMS.sln                             # Visual Studio solution file
├── .gitignore                          # Git ignore configuration
├── README.md                           # Project overview
├── LICENSE                             # License file
│
├── AMS.API/                            # ASP.NET Core Web API
│   ├── AMS.API.csproj                  # Project file
│   ├── Program.cs                      # Startup configuration
│   ├── appsettings.json                # Default configuration
│   ├── appsettings.Development.json    # Dev environment config
│   ├── appsettings.QA.json             # QA environment config
│   ├── appsettings.UAT.json            # UAT environment config
│   ├── appsettings.Production.json     # Production config
│   ├── launchSettings.json             # Launch profiles (IIS, Kestrel)
│   │
│   ├── Controllers/                    # HTTP endpoint handlers
│   │   ├── AttendanceController.cs
│   │   ├── EmployeeController.cs       # (To be implemented)
│   │   ├── LeaveController.cs          # (To be implemented)
│   │   ├── DepartmentController.cs     # (To be implemented)
│   │   └── ReportController.cs         # (To be implemented)
│   │
│   ├── DTOs/                           # Data Transfer Objects
│   │   ├── AttendanceDto.cs
│   │   ├── EmployeeDto.cs
│   │   ├── LeaveDto.cs
│   │   ├── DepartmentDto.cs
│   │   ├── ReportDto.cs
│   │   └── ApiResponseDto.cs
│   │
│   ├── Middleware/                     # Cross-cutting concerns
│   │   ├── ExceptionHandlingMiddleware.cs
│   │   ├── PerformanceMiddleware.cs
│   │   ├── RequestResponseLoggingMiddleware.cs
│   │   ├── RateLimitingMiddleware.cs
│   │   ├── CloudWatchLoggingMiddleware.cs
│   │   ├── ValidationErrorHandlingMiddleware.cs
│   │   └── MiddlewareExtensions.cs     # Middleware registration helper
│   │
│   ├── Filters/                        # Action/Result filters
│   │   └── ValidationExceptionFilter.cs
│   │
│   ├── Extensions/                     # Service configuration
│   │   ├── ServiceCollectionExtensions.cs  # DI registration
│   │   └── ConfigurationExtensions.cs
│   │
│   ├── Validators/                     # FluentValidation rules
│   │   ├── AttendanceValidator.cs
│   │   ├── EmployeeValidator.cs
│   │   └── LeaveValidator.cs
│   │
│   ├── Models/                         # Request/Response models
│   │   ├── LoginRequest.cs
│   │   ├── CreateAttendanceRequest.cs
│   │   └── ApiResponse.cs
│   │
│   └── bin/                            # Compiled binaries (gitignored)
│   └── obj/                            # Intermediate build files (gitignored)
│
├── AMS.Authentication/                 # Authentication & Authorization Layer
│   ├── AMS.Authentication.csproj       # Project file
│   │
│   ├── Configuration/                  # Security settings
│   │   └── AuthenticationSettings.cs   # JWT, password, lockout configs
│   │
│   ├── DTOs/                           # Data Transfer Objects
│   │   └── AuthenticationDtos.cs       # Login, Register, Token responses
│   │
│   ├── Models/                         # Domain models
│   │   ├── AppUser.cs                  # Extended IdentityUser
│   │   ├── AppRole.cs                  # Extended IdentityRole
│   │   ├── RefreshToken.cs             # OAuth 2.0 refresh token
│   │   └── RolePermission.cs           # Fine-grained permissions
│   │
│   ├── Services/                       # Core services
│   │   ├── Interfaces/                 # Service contracts
│   │   │   ├── IAuthenticationService.cs
│   │   │   ├── ITokenService.cs
│   │   │   └── IRoleService.cs
│   │   ├── AuthenticationService.cs    # Login, register, password mgmt
│   │   ├── TokenService.cs             # JWT generation & validation
│   │   └── RoleService.cs              # Role & permission management
│   │
│   ├── Validators/                     # FluentValidation rules
│   │   └── AuthenticationValidators.cs # Login, register, password validators
│   │
│   ├── Extensions/                     # DI & configuration
│   │   └── AuthenticationExtensions.cs # Service registration & policies
│   │
│   ├── README.md                       # Authentication documentation
│   │
│   └── obj/                            # Intermediate build files (gitignored)
│
├── AMS.Repository/                     # Data Access Layer
│   ├── AMS.Repository.csproj
│   │
│   ├── Context/                        # Entity Framework DbContext
│   │   └── AttendanceManagementContext.cs
│   │
│   ├── Entities/                       # Database entity models
│   │   ├── Employee.cs
│   │   ├── Department.cs
│   │   ├── Attendance.cs
│   │   └── Leave.cs
│   │
│   ├── Configuration/                  # Fluent API mappings
│   │   ├── EmployeeConfiguration.cs
│   │   ├── DepartmentConfiguration.cs
│   │   ├── AttendanceConfiguration.cs
│   │   └── LeaveConfiguration.cs
│   │
│   ├── Repository/                     # Repository implementations
│   │   ├── Repository.cs               # Generic base repository
│   │   ├── EmployeeRepository.cs
│   │   ├── AttendanceRepository.cs
│   │   ├── DepartmentRepository.cs
│   │   └── LeaveRepository.cs
│   │
│   ├── Interfaces/                     # Repository contracts
│   │   ├── IRepository.cs              # Generic interface
│   │   ├── IEmployeeRepository.cs
│   │   ├── IAttendanceRepository.cs
│   │   ├── IDepartmentRepository.cs
│   │   ├── ILeaveRepository.cs
│   │   └── IUnitOfWork.cs
│   │
│   ├── UnitOfWork/                     # Unit of Work pattern
│   │   └── UnitOfWork.cs
│   │
│   ├── Extensions/                     # DI and helper methods
│   │   └── ServiceCollectionExtensions.cs
│   │
│   ├── Migrations/                     # Database migrations
│   │   ├── 001_CreateDepartmentTable.sql
│   │   ├── 002_CreateEmployeeTable.sql
│   │   ├── 003_CreateAttendanceTable.sql
│   │   ├── 004_CreateLeaveTable.sql
│   │   ├── DataInitializer.cs          # Migration runner
│   │   └── MIGRATION_SETUP.md          # Migration documentation
│   │
│   └── bin/                            # Compiled binaries (gitignored)
│   └── obj/                            # Intermediate build files (gitignored)
│
├── AMS.Command/                        # Write Operations (CQRS)
│   ├── AMS.Command.csproj
│   ├── Handlers/                       # Command handlers
│   │   ├── CreateAttendanceCommandHandler.cs
│   │   ├── UpdateAttendanceCommandHandler.cs
│   │   └── DeleteAttendanceCommandHandler.cs
│   │
│   ├── Commands/                       # Command objects
│   │   ├── CreateAttendanceCommand.cs
│   │   ├── UpdateAttendanceCommand.cs
│   │   └── DeleteAttendanceCommand.cs
│   │
│   └── Services/                       # Business logic
│       └── AttendanceService.cs        # (To be implemented)
│
├── AMS.Query/                          # Read Operations (CQRS)
│   ├── AMS.Query.csproj
│   ├── Handlers/                       # Query handlers
│   │   ├── GetAttendanceByIdQueryHandler.cs
│   │   ├── GetEmployeeAttendanceQueryHandler.cs
│   │   └── GetAttendanceReportQueryHandler.cs
│   │
│   ├── Queries/                        # Query objects
│   │   ├── GetAttendanceByIdQuery.cs
│   │   ├── GetEmployeeAttendanceQuery.cs
│   │   └── GetAttendanceReportQuery.cs
│   │
│   └── Services/                       # Business logic
│       └── AttendanceQueryService.cs   # (To be implemented)
│
├── Docs/                               # Documentation (this folder)
│   ├── README.md                       # Quick start and overview
│   ├── SETUP_GUIDE.md                  # Installation guide
│   ├── ARCHITECTURE.md                 # System architecture
│   ├── API_DOCUMENTATION.md            # API endpoints reference
│   ├── DATABASE.md                     # Database schema
│   ├── SECURITY.md                     # Security configuration
│   ├── MIDDLEWARE_DOCUMENTATION.md     # Middleware details
│   ├── DEPLOYMENT.md                   # Deployment procedures
│   ├── TROUBLESHOOTING.md              # Common issues & solutions
│   ├── TESTING.md                      # Testing guidelines
│   ├── PROJECT_STRUCTURE.md            # This file
│   └── CONTRIBUTING.md                 # Contribution guidelines
│
└── AMS.Tests/                          # Unit and Integration Tests (To be created)
    ├── AMS.Tests.csproj
    ├── Unit/                           # Unit tests
    │   ├── RepositoryTests/
    │   │   └── EmployeeRepositoryTests.cs
    │   ├── ValidatorTests/
    │   │   └── EmployeeValidatorTests.cs
    │   └── HandlerTests/
    │       └── CreateAttendanceCommandHandlerTests.cs
    │
    ├── Integration/                    # Integration tests
    │   ├── ControllerTests/
    │   │   └── EmployeeControllerTests.cs
    │   └── RepositoryTests/
    │       └── EmployeeRepositoryIntegrationTests.cs
    │
    ├── Fixtures/                       # Test fixtures and helpers
    │   ├── TestDbContextFactory.cs
    │   └── TestDataGenerator.cs
    │
    └── Mocks/                          # Mock implementations
        └── MockUnitOfWork.cs
```

## File Descriptions

### Configuration Files

| File | Purpose |
|------|---------|
| `AMS.sln` | Solution file for Visual Studio |
| `AMS.API/AMS.API.csproj` | Web API project configuration |
| `appsettings.json` | Default configuration values |
| `appsettings.Development.json` | Development-specific config |
| `appsettings.Production.json` | Production-specific config |
| `launchSettings.json` | Debug launch profiles |
| `.gitignore` | Git ignore rules |

### Core Application Files

| File | Purpose |
|------|---------|
| `Program.cs` | Application startup and configuration |
| `*.Controller.cs` | HTTP endpoint handlers returning results |
| `*.Dto.cs` | Data Transfer Objects for API |
| `*.Middleware.cs` | Cross-cutting concern handlers |
| `*Context.cs` | Database context and entity mappings |
| `*Repository.cs` | Data access abstraction |

### Migration Files

| File | Purpose |
|------|---------|
| `001_CreateDepartmentTable.sql` | Create department table |
| `002_CreateEmployeeTable.sql` | Create employee table |
| `003_CreateAttendanceTable.sql` | Create attendance table |
| `004_CreateLeaveTable.sql` | Create leave table |
| `DataInitializer.cs` | Executes migrations on startup |

### Build Output

| Folder | Contents |
|--------|----------|
| `bin/` | Compiled DLLs (Debug or Release) |
| `obj/` | Intermediate build files |

These are **gitignored** and regenerated on build.

## Namespace Structure

```csharp
// API Layer
AMS.API
  AMS.API.Controllers
  AMS.API.DTOs
  AMS.API.Middleware
  AMS.API.Validators
  AMS.API.Models
  AMS.API.Extensions

// Data Layer
AMS.Repository
  AMS.Repository.Context
  AMS.Repository.Entities
  AMS.Repository.Repository
  AMS.Repository.Interfaces
  AMS.Repository.Configuration
  AMS.Repository.Migrations

// Command/Query Layer
AMS.Command
  AMS.Command.Handlers
  AMS.Command.Commands
  AMS.Command.Services

AMS.Query
  AMS.Query.Handlers
  AMS.Query.Queries
  AMS.Query.Services

// Tests
AMS.Tests
  AMS.Tests.Unit
  AMS.Tests.Integration
```

## Typical Workflow

### Adding a New Feature

1. **Create Entity** (`AMS.Repository/Entities/`)
   ```csharp
   public class Report { /* ... */ }
   ```

2. **Create Entity Configuration** (`AMS.Repository/Configuration/`)
   ```csharp
   public class ReportConfiguration : IEntityTypeConfiguration<Report> { /* ... */ }
   ```

3. **Create Repository Interface** (`AMS.Repository/Interfaces/`)
   ```csharp
   public interface IReportRepository : IRepository<Report> { /* ... */ }
   ```

4. **Implement Repository** (`AMS.Repository/Repository/`)
   ```csharp
   public class ReportRepository : Repository<Report>, IReportRepository { /* ... */ }
   ```

5. **Create DTOs** (`AMS.API/DTOs/`)
   ```csharp
   public class ReportDto { /* ... */ }
   ```

6. **Create Validators** (`AMS.API/Validators/`)
   ```csharp
   public class ReportValidator : AbstractValidator<ReportDto> { /* ... */ }
   ```

7. **Create Controller** (`AMS.API/Controllers/`)
   ```csharp
   [ApiController]
   [Route("api/[controller]")]
   public class ReportController : ControllerBase { /* ... */ }
   ```

8. **Create Tests** (`AMS.Tests/`)
   ```csharp
   public class ReportRepositoryTests { /* ... */ }
   ```

9. **Update Documentation** (`Docs/`)
   - Update API_DOCUMENTATION.md with new endpoints
   - Update DATABASE.md if schema changed

## Code Organization Principles

### Single Responsibility
Each file has one primary responsibility:
- Controllers: HTTP request/response
- Repositories: Data access
- Validators: Input validation
- DTOs: Data transfer
- Services: Business logic

### Dependency Injection
Dependencies injected through constructors:
```csharp
public class EmployeeController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EmployeeController> _logger;

    public EmployeeController(IUnitOfWork unitOfWork, ILogger<EmployeeController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
}
```

### Layering
Clear separation between layers:
- **Presentation**: Controllers, DTOs, Middleware
- **Business**: Services, Validators, Handlers
- **Data Access**: Repositories, DbContext, Entities
- **Database**: PostgreSQL tables

### Naming Conventions

| Item | Convention | Example |
|------|-----------|---------|
| Namespace | `AMS.LayerName` | `AMS.Repository.Entities` |
| Class | PascalCase | `EmployeeRepository` |
| Interface | I + PascalCase | `IEmployeeRepository` |
| Method | PascalCase | `GetByIdAsync()` |
| Property | PascalCase | `FirstName` |
| Variable | camelCase | `employeeId` |
| Constant | UPPER_CASE | `MAX_EMPLOYEES` |
| Private field | _camelCase | `_logger` |

## Key Directories

### AMS.API
**Purpose**: HTTP API entry point

**Key Concepts**:
- Every public method in Controller returns `IActionResult`
- Middleware executes for every request
- DTOs abstract entity details from API consumers
- Validators ensure data quality

### AMS.Repository
**Purpose**: Data persistence and access

**Key Concepts**:
- Entities represent database tables
- DbContext maps entities to tables
- Repositories abstract data access
- UnitOfWork coordinates multiple repositories

### AMS.Command
**Purpose**: Write operations (CQRS pattern)

**Key Concepts**:
- Commands represent write intentions
- Handlers execute business logic
- Services contain reusable logic

### AMS.Query
**Purpose**: Read operations (CQRS pattern)

**Key Concepts**:
- Queries represent read requests
- Handlers execute retrieval logic
- Services optimize data access

## Configuration Hierarchy

```
appsettings.json (Base defaults)
         ↓
appsettings.{Environment}.json (Environment overrides)
         ↓
Environment Variables (Runtime overrides)
         ↓
Secrets Manager (Production secrets)
```

## Build & Compilation

### Debug Build
```bash
dotnet build  # Creates bin/Debug/
```

### Release Build
```bash
dotnet build -c Release  # Creates bin/Release/, optimized
```

### Publishing
```bash
dotnet publish -c Release -o ./publish/  # Ready for deployment
```

---

**Last Updated**: March 8, 2024  
**Project Version**: 1.0  
**Structure Pattern**: Layered 3-tier architecture with CQRS ready
