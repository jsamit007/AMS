## Attendance Management System (AMS) - API Documentation

### Base URL
```
https://api.ams.com/api
```

---

## 1. Attendance Endpoints

### Mark Attendance
- **Method**: `POST`
- **Endpoint**: `/attendance/mark`
- **Description**: Records employee check-in/check-out
- **Request Body**: `CreateAttendanceDto`
- **Response**: `ApiResponse<AttendanceDto>`

### Get Attendance Record
- **Method**: `GET`
- **Endpoint**: `/attendance/{id}`
- **Description**: Retrieves specific attendance record
- **Response**: `ApiResponse<AttendanceDto>`

### Get Employee Attendance
- **Method**: `GET`
- **Endpoint**: `/attendance/employee/{employeeId}?pageNumber=1&pageSize=10`
- **Description**: Get all attendance records for an employee
- **Response**: `ApiResponse<PaginatedResponse<AttendanceDto>>`

### Get Attendance by Date Range
- **Method**: `GET`
- **Endpoint**: `/attendance/range?fromDate=2024-01-01&toDate=2024-01-31&pageNumber=1&pageSize=10`
- **Description**: Get attendance records within date range
- **Response**: `ApiResponse<PaginatedResponse<AttendanceDto>>`

### Update Attendance
- **Method**: `PUT`
- **Endpoint**: `/attendance/{id}`
- **Description**: Update attendance record
- **Request Body**: `UpdateAttendanceDto`
- **Response**: `ApiResponse<AttendanceDto>`

### Delete Attendance
- **Method**: `DELETE`
- **Endpoint**: `/attendance/{id}`
- **Description**: Delete attendance record
- **Response**: `ApiResponse<bool>`

---

## 2. Employee Endpoints

### Get Employee
- **Method**: `GET`
- **Endpoint**: `/employees/{id}`
- **Description**: Get employee details
- **Response**: `ApiResponse<EmployeeDto>`

### Get All Employees
- **Method**: `GET`
- **Endpoint**: `/employees?pageNumber=1&pageSize=10`
- **Description**: Get all employees with pagination
- **Response**: `ApiResponse<PaginatedResponse<EmployeeDto>>`

### Get Employees by Department
- **Method**: `GET`
- **Endpoint**: `/employees/department/{departmentId}?pageNumber=1&pageSize=10`
- **Description**: Get employees from specific department
- **Response**: `ApiResponse<PaginatedResponse<EmployeeDto>>`

### Get Employee by Code
- **Method**: `GET`
- **Endpoint**: `/employees/code/{code}`
- **Description**: Get employee by employee code
- **Response**: `ApiResponse<EmployeeDto>`

### Create Employee
- **Method**: `POST`
- **Endpoint**: `/employees`
- **Description**: Create new employee
- **Request Body**: `CreateEmployeeDto`
- **Response**: `ApiResponse<EmployeeDto>`

### Update Employee
- **Method**: `PUT`
- **Endpoint**: `/employees/{id}`
- **Description**: Update employee information
- **Request Body**: `UpdateEmployeeDto`
- **Response**: `ApiResponse<EmployeeDto>`

### Delete Employee
- **Method**: `DELETE`
- **Endpoint**: `/employees/{id}`
- **Description**: Delete/deactivate employee
- **Response**: `ApiResponse<bool>`

---

## 3. Leave Endpoints

### Get Leave Request
- **Method**: `GET`
- **Endpoint**: `/leaves/{id}`
- **Description**: Get leave request details
- **Response**: `ApiResponse<LeaveDto>`

### Get Leave by Employee
- **Method**: `GET`
- **Endpoint**: `/leaves/employee/{employeeId}?pageNumber=1&pageSize=10`
- **Description**: Get all leave requests for employee
- **Response**: `ApiResponse<PaginatedResponse<LeaveDto>>`

### Get Leave by Status
- **Method**: `GET`
- **Endpoint**: `/leaves/status/{status}?pageNumber=1&pageSize=10`
- **Description**: Get leave requests by status (Pending, Approved, Rejected)
- **Response**: `ApiResponse<PaginatedResponse<LeaveDto>>`

### Apply for Leave
- **Method**: `POST`
- **Endpoint**: `/leaves/apply`
- **Description**: Submit new leave request
- **Request Body**: `CreateLeaveDto`
- **Response**: `ApiResponse<LeaveDto>`

### Approve Leave
- **Method**: `PUT`
- **Endpoint**: `/leaves/{id}/approve`
- **Description**: Approve pending leave request
- **Query Parameters**: `approvedBy` (Manager ID)
- **Response**: `ApiResponse<LeaveDto>`

### Reject Leave
- **Method**: `PUT`
- **Endpoint**: `/leaves/{id}/reject`
- **Description**: Reject leave request
- **Query Parameters**: `approvedBy` (Manager ID)
- **Response**: `ApiResponse<LeaveDto>`

### Get Leave Balance
- **Method**: `GET`
- **Endpoint**: `/leaves/balance/{employeeId}`
- **Description**: Get employee's leave balance
- **Response**: `ApiResponse<List<LeaveBalanceDto>>`

### Delete Leave
- **Method**: `DELETE`
- **Endpoint**: `/leaves/{id}`
- **Description**: Delete leave request
- **Response**: `ApiResponse<bool>`

---

## 4. Department Endpoints

### Get Department
- **Method**: `GET`
- **Endpoint**: `/departments/{id}`
- **Description**: Get department details
- **Response**: `ApiResponse<DepartmentDto>`

### Get All Departments
- **Method**: `GET`
- **Endpoint**: `/departments?pageNumber=1&pageSize=10`
- **Description**: Get all departments
- **Response**: `ApiResponse<PaginatedResponse<DepartmentDto>>`

### Get Department by Code
- **Method**: `GET`
- **Endpoint**: `/departments/code/{code}`
- **Description**: Get department by code
- **Response**: `ApiResponse<DepartmentDto>`

### Create Department
- **Method**: `POST`
- **Endpoint**: `/departments`
- **Description**: Create new department
- **Request Body**: `CreateDepartmentDto`
- **Response**: `ApiResponse<DepartmentDto>`

### Update Department
- **Method**: `PUT`
- **Endpoint**: `/departments/{id}`
- **Description**: Update department
- **Request Body**: `UpdateDepartmentDto`
- **Response**: `ApiResponse<DepartmentDto>`

### Delete Department
- **Method**: `DELETE`
- **Endpoint**: `/departments/{id}`
- **Description**: Delete department
- **Response**: `ApiResponse<bool>`

---

## 5. Report Endpoints

### Get Employee Attendance Report
- **Method**: `GET`
- **Endpoint**: `/reports/attendance/employee/{employeeId}?fromDate=2024-01-01&toDate=2024-01-31`
- **Description**: Generate attendance report for employee
- **Response**: `ApiResponse<AttendanceReportDto>`

### Get Department Attendance Report
- **Method**: `GET`
- **Endpoint**: `/reports/attendance/department/{departmentId}?fromDate=2024-01-01&toDate=2024-01-31`
- **Description**: Generate attendance report for department
- **Response**: `ApiResponse<DepartmentAttendanceReportDto>`

### Get All Attendance Reports
- **Method**: `GET`
- **Endpoint**: `/reports/attendance?fromDate=2024-01-01&toDate=2024-01-31&pageNumber=1&pageSize=10`
- **Description**: Get all attendance reports
- **Response**: `ApiResponse<PaginatedResponse<AttendanceReportDto>>`

### Get Employee Leave Balance
- **Method**: `GET`
- **Endpoint**: `/reports/leave-balance/{employeeId}`
- **Description**: Get employee leave balance
- **Response**: `ApiResponse<LeaveBalanceDto>`

### Get Department Leave Balance
- **Method**: `GET`
- **Endpoint**: `/reports/leave-balance/department/{departmentId}`
- **Description**: Get department-wide leave balance
- **Response**: `ApiResponse<List<LeaveBalanceDto>>`

### Export Attendance Report
- **Method**: `GET`
- **Endpoint**: `/reports/attendance/export/{employeeId}?fromDate=2024-01-01&toDate=2024-01-31&format=pdf`
- **Description**: Export report in specified format (pdf, excel, csv)
- **Response**: Binary file

---

## Request/Response Format

### Successful Response Example
```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": {
    "id": 1,
    "employeeId": 101,
    "date": "2024-01-15",
    "checkInTime": "09:00:00",
    "checkOutTime": "17:30:00",
    "status": "Present",
    "remarks": "Normal attendance"
  },
  "errors": []
}
```

### Error Response Example
```json
{
  "success": false,
  "message": "Operation failed",
  "data": null,
  "errors": [
    "Employee not found",
    "Invalid date format"
  ]
}
```

### Paginated Response Example
```json
{
  "success": true,
  "message": "Data retrieved successfully",
  "data": {
    "items": [...],
    "pageNumber": 1,
    "pageSize": 10,
    "totalRecords": 150,
    "totalPages": 15
  },
  "errors": []
}
```

---

## Status Codes

| Code | Meaning |
|------|---------|
| 200 | OK - Request successful |
| 201 | Created - Resource created |
| 400 | Bad Request - Invalid input |
| 404 | Not Found - Resource not found |
| 409 | Conflict - Business logic violation |
| 500 | Internal Server Error |

---

## Authentication

All endpoints require:
- **Authorization Header**: `Bearer <JWT_TOKEN>`
- **Content-Type**: `application/json`

---

## Error Handling

All API errors follow the standard error response format with appropriate HTTP status codes and descriptive error messages.
