# CloudWatch Logging Configuration

## Overview

The Attendance Management System (AMS) API includes comprehensive AWS CloudWatch logging integration for centralized log management, monitoring, and troubleshooting.

## Features

- **Centralized Logging** - All logs aggregated in AWS CloudWatch
- **Log Groups & Streams** - Organized by environment and server
- **Multiple Output Targets** - Console, File, and CloudWatch simultaneously
- **Serilog Integration** - Structured logging with Serilog and Extensions
- **Performance Tracking** - Request timing and performance metrics
- **Error Tracking** - Detailed exception and error logging
- **Environment-Specific Config** - Different log levels per environment

## Architecture

### Log Flow

```
Application
    ↓
ILogger/Serilog
    ↓
    ├─→ Console (all environments)
    ├─→ File Logs (all environments)
    └─→ CloudWatch (QA, UAT, Production)
    
CloudWatch
    ↓
    ├─→ Log Insights (queries)
    ├─→ Alarms (monitoring)
    ├─→ Dashboards (visualization)
    └─→ Retention Policies
```

## Configuration

### appsettings.json Configuration

```json
{
  "CloudWatch": {
    "Enabled": true,
    "LogGroupName": "/ams/api/production",
    "LogStreamName": "prod-server",
    "Region": "us-east-1",
    "AccessKey": "YOUR_AWS_ACCESS_KEY",
    "SecretKey": "YOUR_AWS_SECRET_KEY"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Environment Variables

Set CloudWatch credentials via environment variables (recommended for production):

```bash
# AWS Credentials (alternative to appsettings)
export AWS_ACCESS_KEY_ID="your-access-key"
export AWS_SECRET_ACCESS_KEY="your-secret-key"
export AWS_REGION="us-east-1"

# CloudWatch Configuration
export CloudWatch__Enabled=true
export CloudWatch__LogGroupName=/ams/api/production
export CloudWatch__LogStreamName=prod-server
export CloudWatch__Region=us-east-1
```

### Environment-Specific Settings

#### Development
```json
"CloudWatch": {
  "Enabled": false,
  "LogGroupName": "/ams/api/development",
  "LogStreamName": "local-dev",
  "Region": "us-east-1"
}
```
- CloudWatch disabled by default
- Logs to console and file only
- Log level: Information

#### QA
```json
"CloudWatch": {
  "Enabled": true,
  "LogGroupName": "/ams/api/qa",
  "LogStreamName": "qa-server",
  "Region": "us-east-1"
}
```
- CloudWatch enabled
- Detailed logging
- Log level: Information

#### UAT
```json
"CloudWatch": {
  "Enabled": true,
  "LogGroupName": "/ams/api/uat",
  "LogStreamName": "uat-server",
  "Region": "us-east-1"
}
```
- CloudWatch enabled
- Reduced logging
- Log level: Warning

#### Production
```json
"CloudWatch": {
  "Enabled": true,
  "LogGroupName": "/ams/api/production",
  "LogStreamName": "prod-server",
  "Region": "us-east-1"
}
```
- CloudWatch enabled
- Minimal logging
- Log level: Error
- Retention: 30 days (configurable)

## Setup Instructions

### 1. AWS Prerequisites

Ensure you have:
- AWS account with CloudWatch access
- IAM user with CloudWatchLogs permissions
- Access keys generated

### 2. IAM Policy Required

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "logs:CreateLogGroup",
        "logs:CreateLogStream",
        "logs:PutLogEvents",
        "logs:DescribeLogGroups",
        "logs:DescribeLogStreams"
      ],
      "Resource": "arn:aws:logs:*:*:*"
    }
  ]
}
```

### 3. Configure in Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add API services with CloudWatch logging
builder.Services.AddApiServices(builder.Configuration);

// CloudWatch logging is automatically configured via MiddlewareExtensions

var app = builder.Build();
app.UseApiMiddleware();

app.Run();
```

### 4. Update appsettings.json

```bash
# Set CloudWatch credentials
export AWS_ACCESS_KEY_ID="your-key"
export AWS_SECRET_ACCESS_KEY="your-secret"
export AWS_REGION="us-east-1"
```

Or in appsettings:

```json
{
  "CloudWatch": {
    "Enabled": true,
    "LogGroupName": "/ams/api/production",
    "LogStreamName": "prod-server-01",
    "Region": "us-east-1",
    "AccessKey": "AKIAIOSFODNN7EXAMPLE",
    "SecretKey": "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY"
  }
}
```

### 5. Running the Application

```bash
# Set environment
export ASPNETCORE_ENVIRONMENT=Production

# Set AWS credentials
export AWS_ACCESS_KEY_ID="your-key"
export AWS_SECRET_ACCESS_KEY="your-secret"

# Run application
dotnet run
```

## Log Levels

| Level | Usage | Development | QA | UAT | Production |
|-------|-------|-------------|----|----|------------|
| Trace | Detailed diagnostic info | Ignored | Ignored | Ignored | Ignored |
| Debug | Debug information | Ignored | Ignored | Ignored | Ignored |
| Information | General app flow | ✅ | ✅ |  |  |
| Warning | Warning conditions | ✅ | ✅ | ✅ | ✅ |
| Error | Error conditions | ✅ | ✅ | ✅ | ✅ |
| Critical | Critical failures | ✅ | ✅ | ✅ | ✅ |

## Log Message Format

```
[2024-03-08 15:30:45.123 +00:00] [INF] HTTP Request: GET /api/employees
[2024-03-08 15:30:45.456 +00:00] [INF] Request completed: GET /api/employees in 333ms with status 200
[2024-03-08 15:30:46.789 +00:00] [ERR] An unexpected error occurred: NullReferenceException
```

## Logging in Code

```csharp
public class EmployeeService
{
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(ILogger<EmployeeService> logger)
    {
        _logger = logger;
    }

    public async Task<Employee> GetEmployeeAsync(int employeeId)
    {
        _logger.LogInformation("Fetching employee with ID: {EmployeeId}", employeeId);
        
        try
        {
            var employee = await _repository.GetByIdAsync(employeeId);
            
            if (employee == null)
            {
                _logger.LogWarning("Employee not found: {EmployeeId}", employeeId);
                return null;
            }

            _logger.LogInformation("Successfully retrieved employee: {EmployeeId}", employeeId);
            return employee;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employee: {EmployeeId}", employeeId);
            throw;
        }
    }
}
```

## CloudWatch Insights Queries

### Find Errors in Last Hour
```
fields @timestamp, @message, @logStream
| filter @message like /ERROR/
| stats count() by @logStream
```

### Average Response Time
```
fields @duration
| stats avg(@duration), max(@duration), pct(@duration, 99)
```

### 404 Errors
```
fields @timestamp, @message
| filter @message like /404/
| stats count() by @logStream
```

### Failed Requests
```
fields @timestamp, @message, @logStream
| filter statusCode >= 400
| stats count() by statusCode
```

## Monitoring & Alarms

### Create CloudWatch Alarm for Errors

1. Go to CloudWatch → Alarms → Create Alarm
2. Select Log Group: `/ams/api/production`
3. Create metric filter:
   - Pattern: `[ERROR]`
   - Metric value: `1`
4. Set condition: Error count > 10 in 5 minutes
5. Send notification to SNS topic

### Create Dashboard

```bash
aws cloudwatch put-dashboard \
  --dashboard-name "AMS-API-Dashboard" \
  --dashboard-body file://dashboard-config.json
```

## File Logging

Logs are also written to disk:

```
logs/
├── ams-api-20240308.txt
├── ams-api-20240309.txt
└── ams-api-20240310.txt
```

Retention: 30 days (configurable)

## Troubleshooting

### Issue: Logs not appearing in CloudWatch

**Solution:**
1. Verify CloudWatch is enabled: `"Enabled": true`
2. Check AWS credentials are set correctly
3. Verify IAM permissions
4. Check log group exists in CloudWatch console
5. Review application logs for errors

### Issue: Access Denied errors

**Solution:**
1. Verify AWS Access Key ID and Secret Key
2. Check IAM policy includes CloudWatchLogs permissions
3. Verify AWS region is correct
4. Ensure credentials have not expired

### Issue: High CloudWatch costs

**Solution:**
1. Adjust log retention period
2. Filter log levels appropriately
3. Use sampling for high-volume logs
4. Archive old logs to S3

## Best Practices

1. **Never hardcode credentials** - Use environment variables or IAM roles
2. **Use structured logging** - Include context with each log
3. **Set appropriate log levels** - Avoid DEBUG in production
4. **Monitor costs** - Set retention policies and archive old logs
5. **Use log insights** - Query logs efficiently
6. **Create alarms** - Alert on errors and critical events
7. **Rotate credentials** - Regularly update AWS credentials
8. **Test in non-prod** - Verify configuration before production

## Performance Impact

- **Minimal overhead** - Asynchronous logging to CloudWatch
- **No blocking** - Requests continue while logs are sent
- **Batching** - Logs batched for efficiency
- **Fallback** - Application continues if CloudWatch is unavailable

## Cost Estimation

- **Log ingestion**: $0.50 per GB
- **Log storage**: $0.03 per GB-month
- **Log insights queries**: $0.005 per GB scanned

For typical API doing 1M requests/day:
- ~10-20 GB/month
- Estimated cost: $5-10/month

## Related Documentation

- [AWS CloudWatch Documentation](https://docs.aws.amazon.com/cloudwatch/)
- [AWS CloudWatch Logs Insights](https://docs.aws.amazon.com/AmazonCloudWatch/latest/logs/QuerySyntax.html)
- [Serilog Documentation](https://serilog.net/)
- [MIDDLEWARE_DOCUMENTATION.md](MIDDLEWARE_DOCUMENTATION.md)
- [APPSETTINGS_CONFIGURATION.md](APPSETTINGS_CONFIGURATION.md)
