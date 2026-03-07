# Contributing Guide - AMS

Guidelines for contributing to the Attendance Management System project.

## Code of Conduct

- **Be Respectful**: Treat all team members with respect
- **Be Professional**: Maintain professionalism in all communications
- **Be Collaborative**: Work together to achieve project goals
- **Be Responsible**: Take ownership of your contributions

## Getting Started

### Prerequisites

- .NET 9.0 SDK or higher
- PostgreSQL 12+
- Visual Studio 2022 or VS Code
- Git

### Setup Development Environment

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-org/ams.git
   cd ams
   ```

2. **Follow [SETUP_GUIDE.md](SETUP_GUIDE.md)** for complete setup

3. **Create a feature branch**
   ```bash
   git checkout -b feature/employee-module
   ```

## Development Workflow

### 1. Choose an Issue

- Look for issues labeled `good first issue` or `help wanted`
- Assign issue to yourself
- Discuss approach if needed

### 2. Create Feature Branch

```bash
# Branch naming convention
git checkout -b type/description

# Examples:
git checkout -b feature/add-leave-balance
git checkout -b fix/rate-limiting-bug
git checkout -b docs/update-api-docs
```

**Branch Types**:
- `feature/`: New functionality
- `fix/`: Bug fixes
- `refactor/`: Code restructuring
- `docs/`: Documentation
- `test/`: Test additions
- `perf/`: Performance improvements

### 3. Write Code

#### Follow Code Standards

**Naming Conventions**:
```csharp
// Classes/Methods: PascalCase
public class EmployeeController { }
public async Task<IActionResult> GetEmployeeAsync(int id) { }

// Properties: PascalCase
public string FirstName { get; set; }

// Private fields: _camelCase
private readonly IUnitOfWork _unitOfWork;

// Local variables: camelCase
var employeeId = 1;
```

**Formatting**:
```csharp
// ✅ Good: Readable and formatted
public async Task<IActionResult> GetById(int id)
{
    var employee = await _unitOfWork.Employees.GetByIdAsync(id);
    if (employee == null)
        return NotFound();

    return Ok(employeeMapper.MapToDto(employee));
}

// ❌ Bad: Hard to read
public async Task<IActionResult> GetById(int id){var employee = 
await _unitOfWork.Employees.GetByIdAsync(id);if(employee==null)
return NotFound();return Ok(employeeMapper.MapToDto(employee));}
```

**Comments**:
```csharp
// ✅ Good: Explains WHY
/// <summary>
/// Retrieves employee by ID with attendance records
/// </summary>
/// <param name="id">Employee ID</param>
/// <returns>Employee with attendance history</returns>
public async Task<IActionResult> GetByIdAsync(int id)
{
    // Use SelectAsync to avoid circular reference issues
    var employee = await _unitOfWork.Employees.GetByIdAsync(id);
    return Ok(employee);
}

// ❌ Bad: Obvious comments
int id = 1;  // Set id to 1
var emp = context.Employees.Find(id);  // Get employee
```

#### Architecture Guidelines

**Repository Pattern**:
```csharp
// ✅ Use repositories, not direct DbContext
var employee = await _unitOfWork.Employees.GetByCodeAsync(code);

// ❌ Don't access DbContext directly
var employee = context.Employees.FirstOrDefault(e => e.Code == code);
```

**Dependency Injection**:
```csharp
// ✅ Constructor injection
public class EmployeeService
{
    public EmployeeService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
}

// ❌ Service locator pattern
var service = ServiceProvider.GetService<EmployeeService>();
```

**Async/Await**:
```csharp
// ✅ Use async properly
public async Task<Employee> GetAsync(int id)
{
    return await _unitOfWork.Employees.GetByIdAsync(id);
}

// ❌ Don't block on async
public Employee Get(int id)
{
    return _unitOfWork.Employees.GetByIdAsync(id).Result;  // Blocks!
}
```

### 4. Write Tests

Write tests for new features. Target 80%+ coverage.

```csharp
[Fact]
public async Task CreateEmployee_WithValidData_ReturnsEmployee()
{
    // Arrange
    var mockUnitOfWork = new Mock<IUnitOfWork>();
    var controller = new EmployeeController(mockUnitOfWork.Object);

    // Act
    var result = await controller.CreateAsync(new CreateEmployeeRequest { });

    // Assert
    result.Should().NotBeNull();
    mockUnitOfWork.Verify(u => u.Employees.AddAsync(It.IsAny<Employee>()), Times.Once);
}
```

Run tests locally:
```bash
dotnet test
```

### 5. Update Documentation

Update relevant documentation:
- **API changes**: Update API_DOCUMENTATION.md
- **Database changes**: Update DATABASE.md and migration docs
- **Architecture changes**: Update ARCHITECTURE.md
- **New setup steps**: Update SETUP_GUIDE.md

### 6. Commit Changes

```bash
# Commit with meaningful message
git add .
git commit -m "feat: add leave balance calculation

- Implements leave balance tracking per year
- Adds calculation for annual/casual/sick leave
- Includes validation for leave limits"
```

**Commit Message Format**:
```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types**:
- `feat`: New feature
- `fix`: Bug fix
- `refactor`: Code refactoring
- `docs`: Documentation
- `test`: Test addition
- `perf`: Performance improvement
- `style`: Code style (formatting)
- `ci`: CI/CD changes

**Example**:
```
feat(attendance): add check-in/check-out tracking

Add ability to track employee check-in and check-out times
with status indicators (Present, Late, Absent, Leave).

Includes:
- AttendanceController endpoints
- AttendanceRepository methods
- Database schema migration
- API tests

Fixes #123
```

### 7. Push & Create Pull Request

```bash
git push origin feature/employee-module
```

**Go to GitHub and create a Pull Request**:
- Link related issue (use `Fixes #123`)
- Describe what changed
- Mention any breaking changes
- Request reviewers

**PR Title**:
```
[FEATURE] Add employee module with CRUD operations
[FIX] Resolve rate limiting concurrency issue
[DOCS] Update API documentation for v1.1
```

**PR Description Template**:
```markdown
## Description
Brief description of what this PR does

## Related Issue
Fixes #123

## Changes Made
- Change 1
- Change 2
- Change 3

## Testing
How was this tested? Unit tests? Manual testing?

## Breaking Changes
Any breaking changes to API or database?

## Screenshots
If UI-related, include screenshots

## Checklist
- [ ] Code follows style guidelines
- [ ] Tests added/updated
- [ ] Documentation updated
- [ ] No new warnings generated
```

## Code Review Process

### As Author

1. **Respond to feedback promptly**
2. **Make requested changes**
3. **Push updates to same branch**
4. **Request re-review**

### As Reviewer

1. **Review within 24 hours**
2. **Check for**:
   - ✅ Follows code standards
   - ✅ Tests included
   - ✅ Documentation updated
   - ✅ No performance regressions
   - ✅ No security issues
3. **Approve or request changes**
4. **Merge after approval** (author can self-merge minor docs changes)

## Pull Request Checklist

- [ ] Branch is up-to-date with `main`
- [ ] Code builds without errors: `dotnet build`
- [ ] All tests pass: `dotnet test`
- [ ] Code follows style guide
- [ ] Comments added for complex logic
- [ ] No debug code left behind
- [ ] No secrets/API keys in code
- [ ] Documentation updated
- [ ] PR description is clear
- [ ] Related issues are linked

## Common Contribution Scenarios

### Adding a New API Endpoint

1. **Create DTO** in `AMS.API/DTOs/`
2. **Create Validator** in `AMS.API/Validators/`
3. **Add Repository method** in `AMS.Repository/Repository/`
4. **Add Controller method** in `AMS.API/Controllers/`
5. **Write tests** in `AMS.Tests/`
6. **Update API_DOCUMENTATION.md** with endpoint details
7. **Submit PR**

### Fixing a Bug

1. **Create bug report issue** (if not exists)
2. **Create `fix/` branch**
3. **Write failing test**
4. **Fix the code**
5. **Verify test passes**
6. **Update CHANGELOG** if user-facing
7. **Submit PR linking issue**

### Improving Documentation

1. **Create `docs/` branch**
2. **Make documentation changes**
3. **Build documentation locally** to verify
4. **Submit PR**

### Performance Optimization

1. **Create benchmark** showing current performance
2. **Implement optimization**
3. **Create new benchmark** showing improvement
4. **Include benchmarks in PR**

## Coding Standards

### C# Style Guide

Follow [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)

**Key Points**:
- Use `var` for obvious types: `var employee = new Employee();`
- Use explicit types when unclear: `int employeeId = GetId();`
- Prefer LINQ over loops when appropriate
- Use expression-bodied members for simple properties
- Keep methods focused and small (< 30 lines ideal)

### Async Guidelines

- Method names end with `Async`: `GetByIdAsync()`
- Use `async Task` for void-returning methods only when necessary
- Use `TryAsync` pattern for operations that may fail
- Don't block on async: `.Result` is forbidden

### Error Handling

```csharp
// ✅ Good: Specific exception types
if (employee == null)
    throw new NotFoundException($"Employee {id} not found");

// ❌ Bad: Generic exceptions
if (employee == null)
    throw new Exception("Not found");
```

### LINQ Usage

```csharp
// ✅ Good: Clear and readable
var activeEmployees = context.Employees
    .Where(e => e.IsActive)
    .OrderBy(e => e.FirstName)
    .ToList();

// ❌ Bad: Hard to read
var x = context.Employees.Where(e=>e.IsActive).OrderBy(e=>e.FirstName).ToList();
```

## Performance Guidelines

- ❌ Don't load all records: `context.Employees.ToList()` (1M records)
- ✅ Use pagination: `.Skip(pageIndex).Take(pageSize)`
- ❌ Don't have N+1 queries: `for each { query database }`
- ✅ Use Include: `.Include(e => e.Attendances)`
- ❌ Don't enumerate multiple times: `list.Count()`, then `list.Where()`
- ✅ Use single enumeration: `var filtered = list.Where(...); var count = filtered.Count();`

## Security Guidelines

- ❌ Never hardcode secrets in code
- ✅ Use configuration and environment variables
- ❌ Don't log sensitive data
- ✅ Sanitize user input
- ❌ Don't trust client-side validation
- ✅ Always validate server-side
- ❌ Don't expose internal errors to users
- ✅ Log detailed errors, show generic message to user

## Merge Process

1. **All tests pass**: `dotnet test`
2. **Code review approved**: At least one approval
3. **Branch up-to-date**: Rebase on `main` if needed
4. **No conflicts**: Resolve any merge conflicts
5. **Merge to main**: Squash commits if desired
6. **Delete feature branch**: Cleanup after merge

## Release Process

1. **Create release branch**: `release/v1.1.0`
2. **Update version numbers**: In `.csproj` files
3. **Update CHANGELOG.md**: Document changes
4. **Create tag**: `v1.1.0`
5. **Merge to main**: via PR
6. **Create GitHub Release**: With release notes
7. **Deploy to production**

## Getting Help

- **Questions**: Create discussion in GitHub Discussions
- **Bugs**: Create issue with bug report template
- **Features**: Create issue with feature request template
- **Security**: Email security@yourdomain.com (don't create public issue)
- **Chat**: Ask in Slack #ams-dev

## Review Responses

When receiving feedback:

**Constructive**: Thank reviewer, make change, request re-review
```
Thanks for the feedback! I've updated the method to use async/await 
consistently. Please review when you have a chance.
```

**Disagree**: Explain reasoning respectfully
```
I understand your concern about using async. However, in this case,
we need it for the database query. Could we discuss an alternative approach?
```

**Unclear**: Ask for clarification
```
I'm not sure I understand your suggestion. Could you clarify what you mean
by "extract the validation logic"? Maybe an example would help?
```

## Useful Commands

```bash
# Check code style violations
dotnet format --verify-no-changes

# Auto-format code
dotnet format

# Run tests with coverage
dotnet test /p:CollectCoverage=true

# Build solution
dotnet build

# Clean build artifacts
dotnet clean

# Push branch and track
git push -u origin branch-name

# Update branch from main
git pull origin main
```

## Questions?

1. Check existing documentation
2. Search GitHub issues
3. Ask in Slack #ams-dev
4. Create a discussion on GitHub

## Contributors

See [CONTRIBUTORS.md](CONTRIBUTORS.md) for list of contributors.

---

**Last Updated**: March 8, 2024  
**Branch Protection**: `main` branch requires PR with 1 approval  
**Code Coverage Target**: 80%+  
**Response Time Target**: 24 hours for code review
