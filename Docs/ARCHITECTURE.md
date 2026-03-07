# Architecture - AMS

Comprehensive overview of the Attendance Management System architecture, design patterns, and technical decisions.

## System Architecture Overview

The AMS follows a **3-tier layered architecture** with clear separation of concerns:

```
┌─────────────────────────────────────────────────────┐
│              Presentation Layer                      │
│              (AMS.API - Web API)                    │
│  Controllers, DTOs, Middleware, Error Handling      │
└───────────────────┬─────────────────────────────────┘
                    │
┌───────────────────▼─────────────────────────────────┐
│              Business Logic Layer                    │
│         (AMS.Command & AMS.Query)                   │
│    Services, Validators, Business Rules            │
└───────────────────┬─────────────────────────────────┘
                    │
┌───────────────────▼─────────────────────────────────┐
│         Data Access Layer (AMS.Repository)          │
│   Entity Framework Core, Repositories, UoW          │
└───────────────────┬─────────────────────────────────┘
                    │
┌───────────────────▼─────────────────────────────────┐
│           Database Layer                            │
│              PostgreSQL                             │
│    Tables, Indexes, Constraints, Relationships      │
└─────────────────────────────────────────────────────┘
```

## Layered Architecture Details

### 1. Presentation Layer (AMS.API)

**Purpose**: HTTP API entry point, request/response handling, middleware pipeline

**Components**:
- **Controllers**: HTTP endpoint handlers (AttendanceController, EmployeeController, etc.)
- **DTOs**: Data Transfer Objects for API requests/responses
- **Middleware**: Cross-cutting concerns (logging, auth, rate limiting, CORS, etc.)
- **Program.cs**: Application startup configuration
- **appsettings**: Environment-specific configuration

**Responsibilities**:
- Accept HTTP requests
- Route to appropriate controller methods
- Validate input via attributes/FluentValidation
- Return JSON responses
- Handle HTTP-level concerns

### 2. Business Logic Layer (AMS.Command & AMS.Query)

**Purpose**: Encapsulate business rules and logic (designed for future CQRS separation)

**Components**:
- **Command Handlers**: Write operations (Create, Update, Delete)
- **Query Handlers**: Read operations (Get, Search, Report)
- **Services**: Domain business logic
- **Validators**: FluentValidation rules
- **Specifications**: Complex query specifications

**Responsibilities**:
- Implement business rules
- Validate business logic
- Calculate derived data (leave balance, attendance stats)
- Enforce domain constraints

### 3. Data Access Layer (AMS.Repository)

**Purpose**: Abstract database operations from business logic

**Components**:
- **DbContext**: Entity Framework Core context
- **Entities**: Database entity models
- **Repositories**: Generic and typed repository implementations
- **Unit of Work**: Transaction coordinator
- **Configuration**: Fluent API mappings
- **Migrations**: Database schema versioning

**Key Patterns**:
- **Repository Pattern**: Abstract data access
- **Unit of Work Pattern**: Coordinate multiple repositories
- **Fluent API Configuration**: Type-safe EF Core configuration

### 4. Database Layer

**Technology**: PostgreSQL 12+

**Components**:
- **Tables**: department, employee, attendance, leave
- **Indexes**: Performance optimization
- **Constraints**: Data integrity
- **Relationships**: Foreign keys with cascade policies

## Design Patterns

### 1. Repository Pattern

Abstracts database operations from business logic:

```csharp
public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task SaveChangesAsync();
}
```

**Benefits**:
- Decouples data access from business logic
- Enables unit testing with mock repositories
- Centralizes database queries
- Easy to swap providers (SQL Server → PostgreSQL)

### 2. Unit of Work Pattern

Coordinates multiple repositories with single transaction:

```csharp
public interface IUnitOfWork : IDisposable
{
    IEmployeeRepository Employees { get; }
    IAttendanceRepository Attendances { get; }
    IDepartmentRepository Departments { get; }
    ILeaveRepository Leaves { get; }
    Task SaveChangesAsync();
}
```

**Benefits**:
- Single transaction for multiple operations
- Lazy initialization of repositories
- Consistent state management
- Simplified controller usage

### 3. Dependency Injection

All dependencies resolved via constructor injection:

```csharp
public class AttendanceController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AttendanceController> _logger;

    public AttendanceController(IUnitOfWork unitOfWork, ILogger<AttendanceController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
}
```

**Configured in**: `AMS.Repository/Extensions/ServiceCollectionExtensions.cs`

### 4. Middleware Pipeline Pattern

Sequential processing of HTTP requests:

```
Request → Exception → Performance → Logging → Rate Limit 
  ↓         Handler    Monitor       Middleware → CORS → Auth → 
→ Authorization → Routing → Controller → Response → Logging → Client
```

Each middleware can:
- Transform request
- Short-circuit request
- Log/monitor
- Add context

### 5. CQRS (Command Query Responsibility Segregation)

Separates read and write operations (ready for future implementation):

**Write Path** (AMS.Command):
```
API Request → CommandHandler → Business Logic → Repository → Database
```

**Read Path** (AMS.Query):
```
API Request → QueryHandler → Business Logic → Repository → Database
```

## Data Flow

### Create Attendance Record Flow

```
1. Client sends POST request to /api/attendance
   ↓
2. AttendanceController.CreateAsync() receives request
   ↓
3. Model validation (FluentValidation)
   ↓
4. Call AttendanceService.CreateAsync()
   ↓
5. Business rule validation (employee exists, no duplicate for date)
   ↓
6. Call _unitOfWork.Attendances.AddAsync()
   ↓
7. Call _unitOfWork.SaveChangesAsync()
   ↓
8. EF Core generates INSERT SQL
   ↓
9. PostgreSQL executes and returns generated ID
   ↓
10. Return 201 Created with new record
```

### Retrieve Attendance Records Flow

```
1. Client sends GET request to /api/attendance/employee/{id}
   ↓
2. AttendanceController.GetByEmployeeAsync() receives request
   ↓
3. Call _unitOfWork.Attendances.GetByEmployeeAsync(id)
   ↓
4. Repository builds LINQ query with filters
   ↓
5. EF Core translates to SQL
   ↓
6. PostgreSQL executes SELECT with JOINs and indexes
   ↓
7. Results mapped to DTOs
   ↓
8. Return 200 OK with collection
```

## Entity Relationships

```
┌──────────────┐
│ Department   │
│ (id, name)   │
└──────┬───────┘
       │ 1
       │ to
       │ M
       │
┌──────▼───────────────────┐
│ Employee                  │
│ (id, name, dept_id)       │
└──────┬──────┬─────────────┘
       │ 1    │ 1
       │ to   │ to
       │ M    │ M
       │      │
┌──────▼──┐ ┌─▼──────────────┐
│Attendance   │ Leave           │
│(id, emp_id)│ (id, emp_id,    │
│            │  approved_by)   │
└────────────┴────────────────┘
```

## Authentication & Authorization Flow

```
1. Client calls POST /api/auth/login with credentials
   ↓
2. AuthService validates credentials
   ↓
3. Generate JWT token with claims (user ID, roles, permissions)
   ↓
4. Return token in response
   ↓
5. Client includes token in Authorization header: "Bearer {token}"
   ↓
6. JwtBearerMiddleware validates token signature
   ↓
7. Extract claims and populate HttpContext.User
   ↓
8. AuthorizationMiddleware checks role/policy
   ↓
9. Request proceeds to controller (or returns 403 Forbidden)
```

## Error Handling Architecture

```
Try-Catch in Controllers/Services
         ↓
    Create Exception
         ↓
ExceptionHandlingMiddleware catches
         ↓
Log error details
         ↓
Transform to ApiResponse
         ↓
Return HTTP status code (400, 404, 500)
         ↓
Send JSON error response to client
```

## Caching Strategy

Future caching layers can be added:

```
Request → Controller → [Cache Check] → Service → Repository → Database

Response → [Cache Write] → Client
```

**Candidates for caching**:
- Department list (low change frequency)
- Employee by code (frequently accessed)
- Leave balances (calculated once per day/period)

## Database Optimization

### Indexes
- `employee(employee_code)` - Fast employee lookups
- `employee(email)` - Unique email validation
- `attendance(employee_id, date)` - Daily record lookup
- `leave(employee_id, status)` - Leave request filtering

### Query Optimization
- Use `.AsNoTracking()` for read-only queries
- Batch updates with `ExecuteUpdateAsync()`
- Use `Include()` to prevent N+1 queries
- Implement pagination for large result sets

### Connection Pooling
- Configured automatically by Npgsql
- Default pool size: 25 connections
- Connections reused across requests

## Configuration Management

```
appsettings.json (base defaults)
         ↓
appsettings.{Environment}.json (environment overrides)
         ↓
Environment Variables (runtime overrides)
         ↓
Secrets Manager (sensitive production data)
```

**Environments**:
- **Development**: Local testing, all features enabled
- **Staging/QA**: Pre-production testing
- **UAT**: User acceptance testing
- **Production**: Live system

## Security Architecture

### JWT Token Structure

```
Header:   { "alg": "HS256", "typ": "JWT" }
Payload:  {
  "sub": "1",
  "email": "user@example.com",
  "roles": ["Employee"],
  "iat": 1516239022,
  "exp": 1516242622
}
Signature: HMACSHA256(base64(header).base64(payload), secret_key)
```

### Authorization Policies

```csharp
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"))
    .AddPolicy("ManagerOrAbove", policy => policy.RequireRole("Admin", "Manager"))
    .AddPolicy("CanApproveLeave", policy => policy.RequireRole("Admin", "Manager", "HR"));
```

## Logging & Monitoring

### Structured Logging with Serilog

```csharp
logger.Information("Employee {EmployeeId} clocked in at {Time}", 
    employeeId, DateTime.UtcNow);

logger.Error(ex, "Failed to process attendance for employee {EmployeeId}", 
    employeeId);
```

**Log Sinks**:
- Console (development)
- File (rolling logs)
- CloudWatch (production)

## Scalability Considerations

### Horizontal Scaling
- Stateless API (can run multiple instances)
- Load balancer distributes requests
- PostgreSQL with replication
- Distributed caching (Redis) for future

### Vertical Scaling
- Connection pooling optimization
- Database query optimization
- Index tuning
- Memory management

### Database Optimization for Scale
- Partitioning by date for attendance table
- Archive old records
- Materialized views for reports
- Read replicas for analytics

## Deployment Architecture

```
GitHub Repository
    ↓
CI/CD Pipeline (GitHub Actions/Jenkins)
    ↓
Build & Test
    ↓
Docker Image
    ↓
Container Registry
    ↓
Kubernetes/ECS
    ↓
Load Balancer
    ↓
API Instances
    ↓
PostgreSQL RDS
```

## Technology Stack Rationale

| Layer | Technology | Why |
|-------|-----------|-----|
| API | ASP.NET Core 9.0 | Modern, performant, .NET ecosystem |
| ORM | Entity Framework Core 8 | Type-safe, Fluent API, migrations |
| Database | PostgreSQL 12+ | Open-source, reliability, JSON support |
| Logging | Serilog | Structured logging, multiple sinks |
| Authentication | JWT Bearer | Stateless, scalable, RESTful |
| Validation | FluentValidation | Fluent API, reusable rules |
| Cloud | AWS CloudWatch | Monitoring, logs, metrics |

## Future Architecture Improvements

1. **Full CQRS Implementation**
   - Separate read/write models
   - Event sourcing
   - Projections for reporting

2. **Microservices (if needed)**
   - Attendance Service
   - Leave Service
   - Employee Service
   - Reporting Service

3. **API Gateway**
   - Auth, rate limiting, routing
   - Request transformation
   - Protocol translation

4. **Event-Driven Architecture**
   - Message broker (RabbitMQ, Kafka)
   - Event handlers
   - Async processing

5. **Caching Layer**
   - Redis for hot data
   - Cache invalidation strategy
   - Session management

6. **Analytics & Reporting**
   - Data warehouse
   - BI tools integration
   - Real-time dashboards

---

**Last Updated**: March 8, 2024  
**Architecture Version**: 1.0  
**Author**: Development Team
