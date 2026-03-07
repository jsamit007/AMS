# API Documentation - AMS

Complete reference for all API endpoints, request/response formats, and usage examples.

## Base URL

```
Development:  http://localhost:5000
Staging:      https://staging-ams.yourdomain.com/api
Production:   https://ams.yourdomain.com/api
```

## Authentication

All endpoints (except `/auth/login`) require JWT Bearer token authentication.

### Obtaining a Token

**Endpoint**: `POST /api/auth/login`

**Request**:
```json
{
  "email": "user@example.com",
  "password": "password123"
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresIn": 3600,
    "user": {
      "id": 1,
      "email": "user@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "roles": ["Employee"]
    }
  }
}
```

### Using the Token

Include token in Authorization header:

```bash
curl -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  https://localhost:7001/api/attendance/employees/1
```

## Response Format

All API responses follow a consistent structure:

```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": { /* Response data */ },
  "errors": [],
  "timestamp": "2024-03-08T10:30:00Z"
}
```

### Error Responses

**400 Bad Request**:
```json
{
  "success": false,
  "message": "Validation failed",
  "data": null,
  "errors": [
    "First name is required",
    "Email format is invalid"
  ],
  "timestamp": "2024-03-08T10:30:00Z"
}
```

**401 Unauthorized**:
```json
{
  "success": false,
  "message": "Invalid or expired token",
  "data": null,
  "errors": [],
  "timestamp": "2024-03-08T10:30:00Z"
}
```

**403 Forbidden**:
```json
{
  "success": false,
  "message": "Access denied - insufficient permissions",
  "data": null,
  "errors": [],
  "timestamp": "2024-03-08T10:30:00Z"
}
```

**404 Not Found**:
```json
{
  "success": false,
  "message": "Resource not found",
  "data": null,
  "errors": [],
  "timestamp": "2024-03-08T10:30:00Z"
}
```

**500 Internal Server Error**:
```json
{
  "success": false,
  "message": "An error occurred while processing your request",
  "data": null,
  "errors": ["Internal server error"],
  "timestamp": "2024-03-08T10:30:00Z"
}
```

## Attendance Endpoints

### Create Attendance Record

**Endpoint**: `POST /api/attendance`

**Authentication**: Required (Bearer token)

**Authorization**: Admin, Manager, HR, Self (Employee for own record)

**Request**:
```json
{
  "employeeId": 1,
  "date": "2024-03-08",
  "checkInTime": "09:00:00",
  "checkOutTime": "17:30:00",
  "status": "Present",
  "remarks": "Regular working day"
}
```

**Response** (201 Created):
```json
{
  "success": true,
  "message": "Attendance record created successfully",
  "data": {
    "id": 1,
    "employeeId": 1,
    "employeeName": "John Doe",
    "date": "2024-03-08",
    "checkInTime": "09:00:00",
    "checkOutTime": "17:30:00",
    "status": "Present",
    "remarks": "Regular working day",
    "createdDate": "2024-03-08T10:00:00Z"
  }
}
```

### Get Attendance by Employee

**Endpoint**: `GET /api/attendance/employees/{employeeId}`

**Authentication**: Required

**Query Parameters**:
- `fromDate` (optional): YYYY-MM-DD format
- `toDate` (optional): YYYY-MM-DD format
- `pageNumber` (optional): Default 1
- `pageSize` (optional): Default 10

**Example**:
```bash
GET /api/attendance/employees/1?fromDate=2024-01-01&toDate=2024-03-08&pageNumber=1&pageSize=20
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Attendance records retrieved successfully",
  "data": {
    "records": [
      {
        "id": 1,
        "employeeId": 1,
        "date": "2024-03-08",
        "checkInTime": "09:00:00",
        "checkOutTime": "17:30:00",
        "status": "Present",
        "remarks": ""
      }
    ],
    "totalRecords": 50,
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 3
  }
}
```

### Get Attendance by ID

**Endpoint**: `GET /api/attendance/{id}`

**Authentication**: Required

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Attendance record retrieved successfully",
  "data": {
    "id": 1,
    "employeeId": 1,
    "employeeName": "John Doe",
    "date": "2024-03-08",
    "checkInTime": "09:00:00",
    "checkOutTime": "17:30:00",
    "status": "Present",
    "remarks": "",
    "createdDate": "2024-03-08T10:00:00Z",
    "updatedDate": null
  }
}
```

### Update Attendance

**Endpoint**: `PUT /api/attendance/{id}`

**Authentication**: Required

**Authorization**: Admin, Manager, HR (not self-editable except within same day)

**Request**:
```json
{
  "employeeId": 1,
  "checkOutTime": "18:00:00",
  "status": "Present",
  "remarks": "Extended work time"
}
```

**Response** (200 OK): Updated attendance object

### Delete Attendance

**Endpoint**: `DELETE /api/attendance/{id}`

**Authentication**: Required

**Authorization**: Admin only

**Response** (204 No Content)

## Employee Endpoints

### Get All Employees

**Endpoint**: `GET /api/employees`

**Authentication**: Required

**Query Parameters**:
- `departmentId` (optional)
- `isActive` (optional): true/false
- `pageNumber` (optional)
- `pageSize` (optional)

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "employees": [
      {
        "id": 1,
        "employeeCode": "EMP001",
        "firstName": "John",
        "lastName": "Doe",
        "email": "john.doe@company.com",
        "phoneNumber": "+1234567890",
        "departmentId": 1,
        "departmentName": "IT",
        "designation": "Senior Developer",
        "joiningDate": "2020-01-15",
        "isActive": true
      }
    ],
    "totalRecords": 50,
    "pageNumber": 1,
    "pageSize": 10
  }
}
```

### Get Employee by ID

**Endpoint**: `GET /api/employees/{id}`

**Authentication**: Required

**Response** (200 OK): Single employee object

### Create Employee

**Endpoint**: `POST /api/employees`

**Authentication**: Required

**Authorization**: Admin, HR only

**Request**:
```json
{
  "employeeCode": "EMP102",
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "jane.smith@company.com",
  "phoneNumber": "+9876543210",
  "departmentId": 1,
  "designation": "Developer",
  "joiningDate": "2024-03-01"
}
```

**Validation**:
- `employeeCode`: Unique, required
- `email`: Unique, valid email format
- `departmentId`: Must exist
- `joiningDate`: Cannot be in future

**Response** (201 Created): Created employee object

### Update Employee

**Endpoint**: `PUT /api/employees/{id}`

**Authentication**: Required

**Authorization**: Admin, HR, Self (limited fields only)

**Request**:
```json
{
  "firstName": "Jane",
  "lastName": "Smith",
  "phoneNumber": "+9876543210",
  "designation": "Senior Developer"
}
```

**Response** (200 OK): Updated employee object

### Delete Employee

**Endpoint**: `DELETE /api/employees/{id}`

**Authentication**: Required

**Authorization**: Admin only

**Note**: Typically marks as inactive rather than hard delete

**Response** (204 No Content)

## Leave Endpoints

### Create Leave Request

**Endpoint**: `POST /api/leaves`

**Authentication**: Required

**Request**:
```json
{
  "employeeId": 1,
  "leaveType": "Annual",
  "fromDate": "2024-04-01",
  "toDate": "2024-04-05",
  "numberOfDays": 5,
  "reason": "Family vacation"
}
```

**Validation**:
- `fromDate` <= `toDate`
- `numberOfDays` correctly calculated
- Employee has sufficient leave balance
- No overlapping leave requests

**Response** (201 Created):
```json
{
  "success": true,
  "data": {
    "id": 1,
    "employeeId": 1,
    "leaveType": "Annual",
    "fromDate": "2024-04-01",
    "toDate": "2024-04-05",
    "numberOfDays": 5,
    "reason": "Family vacation",
    "status": "Pending",
    "createdDate": "2024-03-08T10:00:00Z"
  }
}
```

### Get Leave Requests

**Endpoint**: `GET /api/leaves`

**Query Parameters**:
- `employeeId` (optional)
- `status` (optional): Pending, Approved, Rejected, Cancelled
- `leaveType` (optional): Annual, Casual, Sick
- `fromDate` (optional)
- `toDate` (optional)

**Response** (200 OK): Array of leave requests

### Get Leave by ID

**Endpoint**: `GET /api/leaves/{id}`

**Response** (200 OK): Single leave request

### Update Leave Request

**Endpoint**: `PUT /api/leaves/{id}`

**Authentication**: Required

**Authorization**: Employee (own request only, if Pending), Manager/Admin

**Request**:
```json
{
  "fromDate": "2024-04-01",
  "toDate": "2024-04-06",
  "numberOfDays": 6,
  "reason": "Extended family vacation"
}
```

**Response** (200 OK): Updated leave object

### Approve/Reject Leave

**Endpoint**: `POST /api/leaves/{id}/approve` or `POST /api/leaves/{id}/reject`

**Authentication**: Required

**Authorization**: Manager, HR, Admin

**Request** (approve):
```json
{
  "approvalNotes": "Approved as per company policy"
}
```

**Response** (200 OK): Updated leave with status "Approved"

### Cancel Leave

**Endpoint**: `POST /api/leaves/{id}/cancel`

**Response** (200 OK): Cancelled leave request

## Department Endpoints

### Get All Departments

**Endpoint**: `GET /api/departments`

**Query Parameters**:
- `isActive` (optional)

**Response** (200 OK):
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "Information Technology",
      "code": "IT",
      "description": "IT Operations",
      "managerId": 5,
      "managerName": "Alice Manager",
      "isActive": true
    }
  ]
}
```

### Get Department by ID

**Endpoint**: `GET /api/departments/{id}`

**Response** (200 OK): Single department with employee count

### Create Department

**Endpoint**: `POST /api/departments`

**Authorization**: Admin only

**Request**:
```json
{
  "name": "Human Resources",
  "code": "HR",
  "description": "Human Resources Department"
}
```

**Response** (201 Created): Created department

### Update Department

**Endpoint**: `PUT /api/departments/{id}`

**Authorization**: Admin, Manager (own department)

**Response** (200 OK): Updated department

### Delete Department

**Endpoint**: `DELETE /api/departments/{id}`

**Authorization**: Admin only

**Response** (204 No Content)

## Report Endpoints

### Attendance Report

**Endpoint**: `GET /api/reports/attendance`

**Query Parameters**:
- `fromDate` (required)
- `toDate` (required)
- `departmentId` (optional)
- `employeeId` (optional)

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "reportPeriod": {
      "fromDate": "2024-01-01",
      "toDate": "2024-01-31"
    },
    "summary": {
      "totalEmployees": 50,
      "presentDays": 1200,
      "absentDays": 50,
      "lateDays": 30
    },
    "details": [
      {
        "employeeId": 1,
        "employeeName": "John Doe",
        "presentDays": 24,
        "absentDays": 1,
        "lateDays": 1
      }
    ]
  }
}
```

### Leave Balance Report

**Endpoint**: `GET /api/reports/leave-balance`

**Query Parameters**:
- `employeeId` (optional)
- `year` (required)

**Response** (200 OK):
```json
{
  "success": true,
  "data": [
    {
      "employeeId": 1,
      "employeeName": "John Doe",
      "year": 2024,
      "annualLeaveAllocated": 20,
      "annualLeaveUsed": 5,
      "annualLeaveBalance": 15,
      "casualLeaveAllocated": 10,
      "casualLeaveUsed": 2,
      "casualLeaveBalance": 8,
      "sickLeaveAllocated": 12,
      "sickLeaveUsed": 1,
      "sickLeaveBalance": 11
    }
  ]
}
```

## Rate Limiting

The API enforces rate limiting based on IP address:

- **Development**: 200 requests per 60 seconds
- **QA**: 150 requests per 60 seconds
- **UAT/Production**: 100 requests per 60 seconds

**Rate Limit Headers**:
```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1615209600
```

When rate limit exceeded: **429 Too Many Requests**

## Pagination

For endpoints returning collections, use pagination parameters:

```
GET /api/employees?pageNumber=2&pageSize=20
```

**Response**:
```json
{
  "data": {
    "items": [ /* items */ ],
    "totalRecords": 150,
    "pageNumber": 2,
    "pageSize": 20,
    "totalPages": 8,
    "hasNextPage": true,
    "hasPreviousPage": true
  }
}
```

## Filtering & Sorting

**Filter Examples**:
```
GET /api/employees?departmentId=1&isActive=true

GET /api/leaves?status=Pending&employeeId=5

GET /api/attendance?status=Late
```

**Sorting** (future enhancement):
```
GET /api/employees?sortBy=firstName&sortOrder=ascending

GET /api/attendance?sortBy=date&sortOrder=descending
```

## API Versioning

Currently using single version (v1). Future versions:

```
GET /api/v1/employees   # Current
GET /api/v2/employees   # Future
```

## HTTP Status Codes

| Code | Meaning | Common Causes |
|------|---------|---------------|
| 200 | OK | Successful read |
| 201 | Created | Successful create |
| 204 | No Content | Successful delete |
| 400 | Bad Request | Invalid input data |
| 401 | Unauthorized | Missing/invalid token |
| 403 | Forbidden | Insufficient permissions |
| 404 | Not Found | Resource doesn't exist |
| 409 | Conflict | Duplicate/constraint violation |
| 429 | Too Many Requests | Rate limit exceeded |
| 500 | Server Error | Internal error |
| 503 | Service Unavailable | DB connection failed |

## Testing with Swagger

1. Navigate to: `https://localhost:7001/swagger`
2. Click **Authorize** button
3. Paste JWT token from login endpoint
4. Click individual endpoint to test
5. See request/response samples

## Code Examples

### JavaScript/Fetch

```javascript
const token = "your-jwt-token";

fetch('https://localhost:7001/api/employees', {
  method: 'GET',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  }
})
.then(response => response.json())
.then(data => console.log(data));
```

### Python/Requests

```python
import requests

headers = {
    'Authorization': f'Bearer {token}',
    'Content-Type': 'application/json'
}

response = requests.get(
    'https://localhost:7001/api/employees',
    headers=headers
)

print(response.json())
```

### curl

```bash
curl -X GET 'https://localhost:7001/api/employees' \
  -H "Authorization: Bearer your-jwt-token" \
  -H "Content-Type: application/json"
```

### C#/HttpClient

```csharp
using (var client = new HttpClient())
{
    client.DefaultRequestHeaders.Authorization = 
        new AuthenticationHeaderValue("Bearer", token);
    
    var response = await client.GetAsync(
        "https://localhost:7001/api/employees");
    
    var content = await response.Content.ReadAsStringAsync();
    Console.WriteLine(content);
}
```

---

**Last Updated**: March 8, 2024  
**API Version**: 1.0  
**Swagger Doc**: https://localhost:7001/swagger
