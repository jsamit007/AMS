-- Script: 003_CreateAttendanceTable
-- Description: Create Attendance table for daily attendance tracking
-- Author: AMS Team
-- Date: 2024-03-08

CREATE TABLE IF NOT EXISTS attendance (
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

CREATE INDEX IF NOT EXISTS idx_attendance_employee_id ON attendance(employee_id);
CREATE INDEX IF NOT EXISTS idx_attendance_date ON attendance(date);
CREATE INDEX IF NOT EXISTS idx_attendance_status ON attendance(status);
CREATE INDEX IF NOT EXISTS idx_attendance_employee_date ON attendance(employee_id, date);
