# Testing Guide - AMS

Comprehensive guide for writing and running tests for the AMS system.

## Testing Strategy

### Test Pyramid

```
         ╱╲
        ╱  ╲     UI Tests (Manual Testing)
       ╱────╲
      ╱      ╲
     ╱        ╲  Integration Tests
    ╱──────────╲
   ╱            ╲
  ╱              ╲ Unit Tests (Most coverage)
 ╱────────────────╲
```

**Coverage Target**:
- Unit Tests: 80%+ coverage
- Integration Tests: Critical paths
- End-to-End Tests: Main user workflows

## Unit Tests

### Setup

Create test project:

```bash
dotnet new xunit -n AMS.Tests
cd AMS.Tests

# Add references
dotnet add reference ../AMS.API/AMS.API.csproj
dotnet add reference ../AMS.Repository/AMS.Repository.csproj

# Add testing packages
dotnet add package Moq
dotnet add package FluentAssertions
dotnet add package xUnit
```

### Example Unit Test

**File**: `AMS.Tests/EmployeeRepositoryTests.cs`

```csharp
using Xunit;
using Moq;
using FluentAssertions;
using AMS.Repository;
using AMS.Repository.Entities;
using AMS.Repository.Interfaces;

public class EmployeeRepositoryTests
{
    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsEmployee()
    {
        // Arrange
        var mockContext = new Mock<AttendanceManagementContext>();
        var repository = new EmployeeRepository(mockContext.Object);
        var employeeId = 1;

        // Act
        var result = await repository.GetByIdAsync(employeeId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(employeeId);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var mockContext = new Mock<AttendanceManagementContext>();
        var repository = new EmployeeRepository(mockContext.Object);

        // Act
        var result = await repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByCodeAsync_WithValidCode_ReturnsEmployee()
    {
        // Arrange
        var mockContext = new Mock<AttendanceManagementContext>();
        var repository = new EmployeeRepository(mockContext.Object);
        var code = "EMP001";

        // Act
        var result = await repository.GetByCodeAsync(code);

        // Assert
        result.Should().NotBeNull();
        result.EmployeeCode.Should().Be(code);
    }
}
```

### Test Naming Convention

```csharp
[MethodUnderTest]_[Scenario]_[ExpectedResult]
```

Examples:
- `GetByIdAsync_WithValidId_ReturnsEmployee`
- `GetByIdAsync_WithInvalidId_ReturnsNull`
- `Create_WithDuplicateEmail_ThrowsException`
- `Delete_WithInvalidId_ReturnsFalse`

## Integration Tests

### Setup with In-Memory Database

```csharp
public class EmployeeControllerIntegrationTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public EmployeeControllerIntegrationTests()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove real DbContext
                    var dbDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AttendanceManagementContext>));
                    if (dbDescriptor != null)
                        services.Remove(dbDescriptor);

                    // Add in-memory database
                    services.AddDbContext<AttendanceManagementContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDb");
                    });
                });
            });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetEmployee_WithValidId_Returns200()
    {
        // Arrange
        var employeeId = 1;

        // Act
        var response = await _client.GetAsync($"/api/employees/{employeeId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("employeeId");
    }

    async Task IAsyncLifetime.InitializeAsync()
    {
        // Setup test data
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AttendanceManagementContext>();
        await context.Database.EnsureCreatedAsync();

        context.Employees.Add(new Employee
        {
            Id = 1,
            EmployeeCode = "EMP001",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            DepartmentId = 1,
            Designation = "Developer",
            JoiningDate = DateTime.Now
        });

        await context.SaveChangesAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AttendanceManagementContext>();
        await context.Database.EnsureDeletedAsync();
    }
}
```

## Mock Objects

### Mocking Repository

```csharp
[Fact]
public async Task CreateAttendance_CallsRepository()
{
    // Arrange
    var mockUnitOfWork = new Mock<IUnitOfWork>();
    var mockAttendanceRepo = new Mock<IAttendanceRepository>();
    
    mockUnitOfWork
        .Setup(u => u.Attendances)
        .Returns(mockAttendanceRepo.Object);

    var controller = new AttendanceController(mockUnitOfWork.Object, /* other deps */);

    // Act
    await controller.CreateAsync(new CreateAttendanceRequest { /* ... */ });

    // Assert
    mockAttendanceRepo.Verify(r => r.AddAsync(It.IsAny<Attendance>()), Times.Once);
}
```

### Mocking Logger

```csharp
[Fact]
public async Task GetEmployee_LogsRequest()
{
    // Arrange
    var mockLogger = new Mock<ILogger<EmployeeController>>();
    var mockUnitOfWork = new Mock<IUnitOfWork>();
    
    var controller = new EmployeeController(mockUnitOfWork.Object, mockLogger.Object);

    // Act
    await controller.GetByIdAsync(1);

    // Assert
    mockLogger.Verify(
        l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()
        ),
        Times.AtLeastOnce
    );
}
```

## Data-Driven Tests

### Theory Tests with Different Data

```csharp
[Theory]
[InlineData("", false)]  // Empty code
[InlineData("EMP001", true)]  // Valid code
[InlineData("emp001", true)]  // Case insensitive
[InlineData("   ", false)]  // Whitespace
public async Task ValidateEmployeeCode_WithVariousInputs(string code, bool expected)
{
    // Arrange
    var validator = new EmployeeCodeValidator();

    // Act
    var result = validator.Validate(code);

    // Assert
    result.Should().Be(expected);
}

[Theory]
[MemberData(nameof(GetTestEmployees))]
public async Task CreateEmployee_WithVariousData(Employee employee)
{
    // Arrange
    var mockUnitOfWork = new Mock<IUnitOfWork>();
    var controller = new EmployeeController(mockUnitOfWork.Object, /* deps */);

    // Act
    var result = await controller.CreateAsync(employee);

    // Assert
    // ... assertions
}

public static IEnumerable<object[]> GetTestEmployees()
{
    yield return new object[] { new Employee { FirstName = "John" } };
    yield return new object[] { new Employee { FirstName = "Jane" } };
}
```

## Validation Tests

```csharp
public class CreateEmployeeValidatorTests
{
    private readonly CreateEmployeeValidator _validator;

    public CreateEmployeeValidatorTests()
    {
        _validator = new CreateEmployeeValidator();
    }

    [Fact]
    public async Task Validate_WithValidEmployee_Succeeds()
    {
        // Arrange
        var request = new CreateEmployeeRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            EmployeeCode = "EMP001",
            DepartmentId = 1,
            Designation = "Developer"
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithEmptyFirstName_FailsValidation()
    {
        // Arrange
        var request = new CreateEmployeeRequest
        {
            FirstName = "",  // Invalid
            LastName = "Doe",
            Email = "john@example.com"
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "FirstName");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("john@")]
    public async Task Validate_WithInvalidEmail_FailsValidation(string email)
    {
        // Arrange
        var request = new CreateEmployeeRequest
        {
            FirstName = "John",
            Email = email
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }
}
```

## Running Tests

### Run All Tests

```bash
dotnet test
```

### Run Specific Test Class

```bash
dotnet test --filter FullyQualifiedName~EmployeeRepositoryTests
```

### Run with Coverage

```bash
# Install coverage tool
dotnet add package coverlet.collector

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover

# View coverage report
# Install ReportGenerator
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate HTML report
reportgenerator -reports:coverage.xml -targetdir:coverage-report
open coverage-report/index.html
```

### Run with Verbose Output

```bash
dotnet test --logger "console;verbosity=detailed"
```

## Test Best Practices

### ✅ DO

- **Use Arrange-Act-Assert pattern**
  ```csharp
  // Good
  [Fact]
  public void Test()
  {
      // Arrange - Setup
      var sut = new SystemUnderTest();
      
      // Act - Execute
      var result = sut.Method();
      
      // Assert - Verify
      result.Should().Be(expected);
  }
  ```

- **Test one thing per test**
  ```csharp
  // Good - One behavior
  [Fact]
  public void GetById_WithValidId_ReturnsUser() { }
  ```

- **Use descriptive names**
  ```csharp
  // Good
  [Fact]
  public void CreateAttendance_WithFutureDate_ThrowsArgumentException() { }
  ```

- **Mock external dependencies**
  ```csharp
  // Good - Database mocked
  var mockDb = new Mock<IUnitOfWork>();
  ```

### ❌ DON'T

- **Test multiple behaviors**
  ```csharp
  // Bad - Tests two things
  [Fact]
  public void GetAndCreate() { }
  ```

- **Use vague names**
  ```csharp
  // Bad
  [Fact]
  public void Test1() { }
  ```

- **Test implementation details**
  ```csharp
  // Bad - Testing private method
  sut.PrivateMethod().Should().NotBeNull();
  ```

- **Have test dependencies**
  ```csharp
  // Bad - Test B depends on Test A
  public void TestA() { state = true; }
  public void TestB() { Assert.True(state); }  // Can fail if A doesn't run
  ```

## Continuous Integration (CI)

### GitHub Actions Example

```yaml
name: Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    
    services:
      postgres:
        image: postgres:15
        env:
          POSTGRES_PASSWORD: postgres
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
        ports:
          - 5432:5432

    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '9.0.x'
    
    - name: Restore
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore
    
    - name: Test
      run: dotnet test --no-build --verbosity normal
      env:
        ConnectionStrings__DefaultConnection: "Host=localhost;Port=5432;Database=ams_test;Username=postgres;Password=postgres"
```

## Test Coverage Goals

| Component | Target | Current |
|-----------|--------|---------|
| Repositories | 90% | 85% |
| Controllers | 85% | 80% |
| Services | 90% | 88% |
| Validators | 95% | 92% |
| Overall | 85% | 82% |

---

**Last Updated**: March 8, 2024  
**Test Framework**: xUnit  
**Mocking Framework**: Moq  
**Assertion Library**: FluentAssertions
