# Attendance Management System (AMS)

Welcome to the Attendance Management System - a comprehensive, enterprise-grade solution for managing employee attendance, leave requests, and related administrative tasks.

## 📋 Overview

The Attendance Management System is a modern, cloud-ready ASP.NET Core 9.0 API built with PostgreSQL, featuring:

- **Real-time Attendance Tracking**: Check-in/check-out management with timestamp precision
- **Leave Management**: Complete leave request workflow with approval chains
- **Department Management**: Organizational structure and hierarchy management
- **JWT Authentication**: Secure token-based authentication with refresh tokens
- **Role-Based Access Control (RBAC)**: Fine-grained permission management with role hierarchy
- **Account Security**: Account lockout, password complexity enforcement, audit trails
- **CloudWatch Integration**: AWS CloudWatch logging for monitoring and diagnostics
- **RESTful API**: Comprehensive API endpoints with Swagger documentation
- **PostgreSQL Database**: Reliable, scalable relational database
- **Entity Framework Core**: Type-safe ORM with Fluent API configuration

## 🎯 Quick Start

### Prerequisites

- **.NET 9.0 SDK** or higher
- **PostgreSQL 12+**
- **Visual Studio 2022** or VS Code
- **Git**

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-org/ams.git
   cd ams
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Setup PostgreSQL database**
   ```bash
   # Create database
   createdb -U postgres ams_dev
   ```

4. **Configure connection string**
   Edit `AMS.API/appsettings.Development.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Port=5432;Database=ams_dev;Username=postgres;Password=postgres;"
   }
   ```

5. **Run the application**
   ```bash
   cd AMS.API
   dotnet run
   ```

6. **Open Swagger UI**
   Navigate to `https://localhost:7001/swagger` in your browser

## 📁 Project Structure

```
AMS/
├── AMS.API/                 # Web API (entry point)
├── AMS.Authentication/      # Authentication & Authorization (JWT, Identity, RBAC)
├── AMS.Repository/          # Data access layer
│   ├── Entities/           # Database entities
│   ├── Context/            # DbContext
│   ├── Repository/         # Repository pattern implementations
│   ├── Migrations/         # SQL migrations
│   └── Configuration/      # EF Core Fluent API
├── AMS.Command/            # Command handlers (write operations)
├── AMS.Query/              # Query handlers (read operations)
├── AMS.Contracts/          # Shared DTOs and validation
├── Docs/                   # Documentation (this folder)
└── AMS.sln                 # Solution file
```

## 🚀 Key Features

### 1. Attendance Management
- Clock in/out tracking
- Daily attendance reports
- Attendance history with timestamps
- Status tracking (Present, Absent, Late, Leave)

### 2. Leave Management
- Leave requests with approval workflow
- Multiple leave types (Annual, Casual, Sick)
- Leave balance tracking
- Approval chain management

### 3. Employee Management
- Employee directory
- Department assignments
- Employee status tracking (Active/Inactive)
- Joining date and designation tracking

### 4. Department Management
- Department hierarchy
- Department descriptions
- Manager assignments
- Active/inactive status

### 5. Security & Authentication
- **JWT Bearer Token Authentication**: Secure token-based authentication
- **Refresh Token Mechanism**: OAuth 2.0 compliant token renewal
- **Role-Based Access Control (RBAC)**: Admin, Manager, User roles with hierarchy
- **Fine-Grained Permissions**: Resource:action format (e.g., "attendance:create")
- **Account Security**: 
  - Password complexity enforcement (8+ chars, mixed case, digits, special chars)
  - Bcrypt password hashing
  - Account lockout after failed login attempts
  - Audit logging of all authentication events
- **Claims-Based Authorization**: Extensible policy definitions
- **CORS Configuration**: Multi-origin access with configurable origins

### 6. Logging & Monitoring
- Structured logging with Serilog
- CloudWatch integration for AWS environments
- Request/response logging
- Performance monitoring
- Error tracking and reporting

## 📖 Documentation

Comprehensive documentation is available in the `Docs/` folder:

- **[SETUP_GUIDE.md](SETUP_GUIDE.md)** - Detailed installation and configuration
- **[ARCHITECTURE.md](ARCHITECTURE.md)** - System architecture and design patterns
- **[API_DOCUMENTATION.md](API_DOCUMENTATION.md)** - Complete API reference
- **[DATABASE.md](DATABASE.md)** - Database schema and relationships
- **[SECURITY.md](SECURITY.md)** - Authentication and authorization
- **[MIDDLEWARE_DOCUMENTATION.md](MIDDLEWARE_DOCUMENTATION.md)** - Middleware components
- **[DEPLOYMENT.md](DEPLOYMENT.md)** - Production deployment guide
- **[TROUBLESHOOTING.md](TROUBLESHOOTING.md)** - Common issues and solutions
- **[TESTING.md](TESTING.md)** - Testing strategies and guidelines
- **[PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md)** - Detailed folder structure
- **[CONTRIBUTING.md](CONTRIBUTING.md)** - Contribution guidelines
- **[MIGRATIONS.md](../AMS.Repository/Migrations/MIGRATION_SETUP.md)** - Database migrations

## 🔧 Configuration

The application supports environment-specific configuration:

- **Development** (`appsettings.Development.json`)
  - Swagger enabled
  - Loose rate limiting (200 req/60s)
  - Local CORS origins
  - CloudWatch disabled

- **QA** (`appsettings.QA.json`)
  - Swagger enabled
  - Moderate rate limiting (150 req/60s)
  - CloudWatch enabled

- **UAT** (`appsettings.UAT.json`)
  - Swagger disabled
  - Strict rate limiting (100 req/60s)
  - CloudWatch enabled

- **Production** (`appsettings.Production.json`)
  - Swagger disabled
  - Strict rate limiting (100 req/60s)
  - CloudWatch enabled
  - Error-only logging

### Environment Variables

Set the `ASPNETCORE_ENVIRONMENT` variable to switch configurations:

```bash
# Windows
set ASPNETCORE_ENVIRONMENT=Development

# Linux/Mac
export ASPNETCORE_ENVIRONMENT=Development
```

## 🗄️ Database

The system uses **PostgreSQL** with the following tables:

- `department` - Organization departments
- `employee` - Employee records
- `attendance` - Daily attendance tracking
- `leave` - Leave requests
- `schemaversions` - Migration tracking (auto-managed by DbUp)

Database migrations execute automatically on application startup via DbUp.

## 🔐 Authentication

The API uses JWT Bearer token authentication. To authenticate:

1. **Obtain a token** via the authentication endpoint
2. **Include the token** in the Authorization header:
   ```
   Authorization: Bearer <your-jwt-token>
   ```

For detailed security information, see [SECURITY.md](SECURITY.md).

## 📊 API Response Format

All API responses follow a consistent format:

```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": {
    "id": 1,
    "employeeCode": "EMP001",
    "firstName": "John",
    "lastName": "Doe"
  },
  "errors": [],
  "timestamp": "2024-03-08T10:30:00Z"
}
```

## 🌐 CORS Configuration

CORS is configured per environment. Configure allowed origins in `appsettings.{Environment}.json`:

```json
"Cors": {
  "AllowedOrigins": [
    "http://localhost:3000",
    "http://localhost:5173",
    "https://yourdomain.com"
  ]
}
```

## 📈 Rate Limiting

Request rate limiting protects the API from abuse:

- **Development**: 200 requests per 60 seconds
- **QA**: 150 requests per 60 seconds
- **UAT/Production**: 100 requests per 60 seconds

Rate limiting is based on IP address and enforced via middleware.

## 🐛 Troubleshooting

For common issues and solutions, refer to [TROUBLESHOOTING.md](TROUBLESHOOTING.md).

## 📝 Logging

The application uses **Serilog** for structured logging with multiple sinks:

- **Console Sink**: Colorized output to console
- **File Sink**: Rolling file logs in `logs/` directory
- **CloudWatch Sink**: AWS CloudWatch Logs (production environments)

View logs in real-time during development:
```bash
dotnet run
```

## 🤝 Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for contribution guidelines, code standards, and pull request procedures.

## 📜 License

This project is licensed under the MIT License - see LICENSE file for details.

## 👥 Support Team

For issues, questions, or feature requests:

- **Documentation**: Review the `Docs/` folder
- **Issue Tracker**: GitHub Issues
- **Email Support**: support@yourdomain.com
- **Slack Channel**: #ams-support

## 🔄 Version Information

- **Current Version**: 1.0.0
- **.NET Version**: 9.0
- **PostgreSQL Version**: 12+
- **Entity Framework Core**: 8.0.0
- **Serilog**: 4.0.0

## 📅 Changelog

See [CHANGELOG.md](CHANGELOG.md) for version history and updates.

---

**Last Updated**: March 8, 2024  
**Maintained By**: AMS Development Team  
**Status**: Active Development
