# Database Schema - AMS

Complete reference for the PostgreSQL database schema, relationships, and design.

## Database Overview

**Database System**: PostgreSQL 12+  
**Connection Driver**: Npgsql for .NET  
**ORM**: Entity Framework Core 8.0  
**Migrations**: DbUp (SQL-based versioning)

## Entity-Relationship Diagram

```
┌─────────────────────────────────────┐
│         DEPARTMENT                  │
├─────────────────────────────────────┤
│ PK  id (INT)                        │
│     name (VARCHAR(100))             │
│     code (VARCHAR(20))              │
│     description (VARCHAR(500))      │
│     manager_id (INT) [FK]           │
│     is_active (BOOLEAN)             │
│     created_date (TIMESTAMP)        │
│     updated_date (TIMESTAMP)        │
└─────────────────────────────────────┘
              │
              │ 1:N
              │
┌─────────────▼──────────────────────────┐
│         EMPLOYEE                       │
├────────────────────────────────────────┤
│ PK  id (INT)                           │
│ FK  department_id (INT)                │
│     employee_code (VARCHAR(20))        │
│     first_name (VARCHAR(50))           │
│     last_name (VARCHAR(50))            │
│     email (VARCHAR(100))               │
│     phone_number (VARCHAR(20))         │
│     designation (VARCHAR(100))         │
│     joining_date (TIMESTAMP)           │
│     is_active (BOOLEAN)                │
│     created_date (TIMESTAMP)           │
│     updated_date (TIMESTAMP)           │
└─────────────┬──────────────┬───────────┘
              │              │
         1:N  │              │  1:N
              │              │
              │    ┌─────────┘
              │    │
┌─────────────▼──┐ │  ┌──────────────────────┐
│ ATTENDANCE     │ │  │ LEAVE                │
├────────────────┤ │  ├──────────────────────┤
│ PK  id (INT)   │ │  │ PK  id (INT)         │
│ FK  employee_  │ │  │ FK  employee_id (INT)│
│     id (INT)   │ │  │ FK  approved_by (INT)│
│     date (DATE)│ │  │     leave_type       │
│     check_in   │ │  │     (VARCHAR(20))    │
│     time (TIME)│ │  │     from_date (DATE) │
│     check_out_ │ │  │     to_date (DATE)   │
│     time (TIME)│ │  │     number_of_days   │
│     status     │ │  │     (INT)            │
│     (VARCHAR)  │ │  │     reason           │
│     remarks    │ │  │     (VARCHAR(500))   │
│     (VARCHAR)  │ │  │     status           │
│     created_   │ │  │     (VARCHAR(20))    │
│     date       │ │  │     approved_date    │
│     updated_   │ │  │     (TIMESTAMP)      │
│     date       │ │  │     created_date     │
└────────────────┘ │  │     updated_date     │
                   │  └──────────────────────┘
                   │
                   └─ refers to EMPLOYEE
```

## Table Details

### department

Represents organizational departments.

**Columns**:
```sql
CREATE TABLE department (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    code VARCHAR(20) NOT NULL UNIQUE,
    description VARCHAR(500),
    manager_id INTEGER,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_date TIMESTAMP
);
```

**Indexes**:
```sql
CREATE INDEX idx_department_code ON department(code);
CREATE INDEX idx_department_is_active ON department(is_active);
```

**Constraints**:
- `PK`: id (primary key)
- `UNIQUE`: name, code
- `FK`: manager_id → employee(id)
- `DEFAULT`: is_active = TRUE, created_date = CURRENT_TIMESTAMP

**Sample Data**:
```
id | name                  | code | is_active
---|-----------------------|------|----------
 1 | Information Technology| IT   | t
 2 | Human Resources       | HR   | t
 3 | Finance               | FIN  | t
 4 | Operations            | OPS  | t
```

### employee

Represents employees in the system.

**Columns**:
```sql
CREATE TABLE employee (
    id SERIAL PRIMARY KEY,
    department_id INTEGER NOT NULL,
    employee_code VARCHAR(20) NOT NULL UNIQUE,
    first_name VARCHAR(50) NOT NULL,
    last_name VARCHAR(50) NOT NULL,
    email VARCHAR(100) NOT NULL UNIQUE,
    phone_number VARCHAR(20) NOT NULL,
    designation VARCHAR(100) NOT NULL,
    joining_date TIMESTAMP NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_date TIMESTAMP,
    FOREIGN KEY (department_id) REFERENCES department(id)
);
```

**Indexes**:
```sql
CREATE INDEX idx_employee_code ON employee(employee_code);
CREATE INDEX idx_employee_email ON employee(email);
CREATE INDEX idx_employee_department_id ON employee(department_id);
CREATE INDEX idx_employee_is_active ON employee(is_active);
```

**Constraints**:
- `PK`: id
- `FK`: department_id → department(id)
- `UNIQUE`: employee_code, email
- `NOT NULL`: department_id, employee_code, first_name, last_name, email, designation

**Sample Data**:
```
id | employee_code | first_name | last_name | email                    | dept_id
---|---------------|------------|-----------|--------------------------|--------
 1 | EMP001        | John       | Doe       | john.doe@company.com     | 1
 2 | EMP002        | Jane       | Smith     | jane.smith@company.com   | 1
 3 | EMP003        | Bob        | Johnson   | bob.johnson@company.com  | 2
```

### attendance

Records daily attendance for each employee.

**Columns**:
```sql
CREATE TABLE attendance (
    id SERIAL PRIMARY KEY,
    employee_id INTEGER NOT NULL,
    date DATE NOT NULL,
    check_in_time TIME NOT NULL,
    check_out_time TIME,
    status VARCHAR(20) NOT NULL,
    remarks VARCHAR(500),
    created_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_date TIMESTAMP,
    FOREIGN KEY (employee_id) REFERENCES employee(id) ON DELETE CASCADE,
    UNIQUE(employee_id, date)
);
```

**Indexes**:
```sql
CREATE INDEX idx_attendance_employee_id ON attendance(employee_id);
CREATE INDEX idx_attendance_date ON attendance(date);
CREATE INDEX idx_attendance_status ON attendance(status);
CREATE INDEX idx_attendance_employee_date ON attendance(employee_id, date);
```

**Constraints**:
- `PK`: id
- `FK`: employee_id → employee(id) ON DELETE CASCADE
- `UNIQUE`: (employee_id, date) - one record per employee per day
- `CHECK` (implicit): status IN ('Present', 'Absent', 'Late', 'Leave', 'Holiday')

**Status Values**:
- `Present`: Regular attendance
- `Absent`: No attendance without leave
- `Late`: Arrived after standard check-in time
- `Leave`: On approved leave
- `Holiday`: Public/company holiday

**Sample Data**:
```
id | employee_id | date       | check_in  | check_out | status  | remarks
---|-------------|------------|-----------|-----------|---------|--------
 1 | 1           | 2024-03-08 | 09:00:00  | 17:30:00  | Present | 
 2 | 1           | 2024-03-07 | 09:15:00  | 17:30:00  | Late    | 
 3 | 2           | 2024-03-08 | 09:00:00  | 17:45:00  | Present | Extended hours
```

### leave

Represents leave requests with approval workflow.

**Columns**:
```sql
CREATE TABLE leave (
    id SERIAL PRIMARY KEY,
    employee_id INTEGER NOT NULL,
    leave_type VARCHAR(20) NOT NULL,
    from_date DATE NOT NULL,
    to_date DATE NOT NULL,
    number_of_days INTEGER NOT NULL,
    reason VARCHAR(500) NOT NULL,
    status VARCHAR(20) NOT NULL,
    approved_by INTEGER,
    approved_date TIMESTAMP,
    created_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_date TIMESTAMP,
    FOREIGN KEY (employee_id) REFERENCES employee(id) ON DELETE CASCADE,
    FOREIGN KEY (approved_by) REFERENCES employee(id) ON DELETE SET NULL
);
```

**Indexes**:
```sql
CREATE INDEX idx_leave_employee_id ON leave(employee_id);
CREATE INDEX idx_leave_status ON leave(status);
CREATE INDEX idx_leave_type ON leave(leave_type);
CREATE INDEX idx_leave_from_date ON leave(from_date);
CREATE INDEX idx_leave_employee_status ON leave(employee_id, status);
```

**Constraints**:
- `PK`: id
- `FK`: employee_id → employee(id) ON DELETE CASCADE
- `FK`: approved_by → employee(id) ON DELETE SET NULL
- `NOT NULL`: employee_id, leave_type, from_date, to_date, number_of_days, reason, status

**Leave Types**:
- `Annual`: Annual/vacation leave (typically 20 days/year)
- `Casual`: Casual leave (typically 10 days/year)
- `Sick`: Sick leave (typically 12 days/year)
- `Maternity`: Maternity leave
- `Paternity`: Paternity leave
- `Unpaid`: Unpaid leave

**Status Values**:
- `Pending`: Awaiting approval
- `Approved`: Approved by manager/HR
- `Rejected`: Rejected by manager/HR
- `Cancelled`: Cancelled by employee or admin
- `Completed`: Leave period has ended

**Sample Data**:
```
id | employee_id | leave_type | from_date  | to_date    | status   | approved_by
---|-------------|-----------|-----------|-----------|----------|-------------
 1 | 1           | Annual    | 2024-04-01 | 2024-04-05| Pending  | NULL
 2 | 2           | Sick      | 2024-03-08 | 2024-03-08| Approved | 3
```

### schemaversions

Auto-managed by DbUp for migration tracking.

**Columns**:
```sql
CREATE TABLE public.schemaversions (
    schemaversionsid SERIAL PRIMARY KEY,
    scriptname VARCHAR(255) NOT NULL,
    applied TIMESTAMP NOT NULL
);
```

**Sample Data**:
```
scriptname                    | applied
------------------------------|---------------------------
001_CreateDepartmentTable.sql | 2024-03-08 10:00:00
002_CreateEmployeeTable.sql   | 2024-03-08 10:00:05
003_CreateAttendanceTable.sql | 2024-03-08 10:00:10
004_CreateLeaveTable.sql      | 2024-03-08 10:00:15
```

## Relationships

### Department → Employee (1:N)

- One department has many employees
- When department is deleted, employees are NOT deleted (integrity constraint)
- Filter employees by department_id

### Employee → Attendance (1:N)

- One employee has many attendance records
- When employee is deleted, attendance records are deleted (CASCADE)
- One unique attendance record per employee per day

### Employee → Leave (1:N)

- One employee has many leave requests
- When employee is deleted, leave requests are deleted (CASCADE)
- Leave can be approved by another employee (manager)

### Employee → Employee (M:1) - Department Manager

- Department can have a manager (employee)
- When manager is deleted, department.manager_id becomes NULL (SET NULL)

## Queries

### Find today's attendance for an employee

```sql
SELECT a.*, e.first_name, e.last_name
FROM attendance a
JOIN employee e ON a.employee_id = e.id
WHERE a.employee_id = 1 AND a.date = CURRENT_DATE;
```

### Find pending leave requests for a department

```sql
SELECT l.*, e.first_name, e.last_name
FROM leave l
JOIN employee e ON l.employee_id = e.id
JOIN department d ON e.department_id = d.id
WHERE d.id = 1 AND l.status = 'Pending'
ORDER BY l.created_date DESC;
```

### Get attendance summary by status

```sql
SELECT 
  status,
  COUNT(*) as total,
  ROUND(COUNT(*) * 100.0 / SUM(COUNT(*)) OVER (), 2) as percentage
FROM attendance
WHERE date >= CURRENT_DATE - INTERVAL '30 days'
GROUP BY status
ORDER BY total DESC;
```

### Find employees by department with attendance this month

```sql
SELECT 
  e.id,
  e.first_name,
  e.last_name,
  d.name as department,
  COUNT(a.id) as attendance_days
FROM employee e
LEFT JOIN department d ON e.department_id = d.id
LEFT JOIN attendance a ON e.id = a.employee_id 
  AND a.date >= DATE_TRUNC('month', CURRENT_DATE)
WHERE e.is_active = TRUE
GROUP BY e.id, e.first_name, e.last_name, d.name
ORDER BY e.first_name;
```

## Performance Optimization

### Index Strategy

- **Covering Indexes**: `(employee_id, status)` on leave table for filtering
- **Date Indexes**: `date` on attendance for range queries
- **Foreign Key Indexes**: Automatic on all FK columns
- **Status Indexes**: Frequently filtered columns

### Query Optimization

```csharp
// ❌ Bad: N+1 query problem
var employees = await context.Employees.ToListAsync();
foreach(var emp in employees) {
    var attendance = await context.Attendances
        .Where(a => a.EmployeeId == emp.Id)
        .ToListAsync();
}

// ✅ Good: Single query with Include
var employees = await context.Employees
    .Include(e => e.Attendances)
    .ToListAsync();

// ✅ Good: Use Select for specific fields
var attendance = await context.Attendances
    .Where(a => a.Date >= fromDate && a.Date <= toDate)
    .Select(a => new {
        a.Id,
        a.EmployeeId,
        a.Date,
        a.Status
    })
    .ToListAsync();
```

### Database Maintenance

```sql
-- Analyze table performance
ANALYZE attendance;

-- Vacuum and reindex
VACUUM ANALYZE employee;
REINDEX TABLE employee;

-- Check index usage
SELECT schemaname, tablename, indexname, idx_scan
FROM pg_stat_user_indexes
ORDER BY idx_scan DESC;
```

## Backup & Recovery

### Backup

```bash
# Full database backup
pg_dump -U postgres -d ams_dev > ams_backup.sql

# Compressed backup
pg_dump -U postgres -d ams_dev | gzip > ams_backup.sql.gz

# With verbose output
pg_dump -U postgres -d ams_dev -v > ams_backup.sql
```

### Restore

```bash
# From SQL dump
psql -U postgres -d ams_dev < ams_backup.sql

# From compressed backup
gunzip -c ams_backup.sql.gz | psql -U postgres -d ams_dev
```

## Data Integrity Constraints

### Business Rules Enforced

1. **One attendance per employee per day** - UNIQUE constraint
2. **Leave dates must be valid** - Application validation (from_date ≤ to_date)
3. **Employee codes are unique** - UNIQUE constraint
4. **Email addresses are unique** - UNIQUE constraint
5. **Department and employee codes are unique** - UNIQUE constraint

### Referential Integrity

- Deleting department → Foreign key prevents unless CASCADE set
- Deleting employee → Cascades delete to attendance and leave records
- Deleting approver → Sets leave.approved_by to NULL

## Monitoring & Statistics

### Check table sizes

```sql
SELECT 
  schemaname,
  tablename,
  pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename)) as size
FROM pg_tables
WHERE schemaname NOT IN ('pg_catalog', 'information_schema')
ORDER BY pg_total_relation_size(schemaname||'.'||tablename) DESC;
```

### Check slow queries

```sql
SELECT 
  query,
  calls,
  total_time,
  mean_time,
  max_time
FROM pg_stat_statements
ORDER BY mean_time DESC
LIMIT 10;
```

---

**Last Updated**: March 8, 2024  
**PostgreSQL Version**: 12+  
**Schema Version**: 1.0
