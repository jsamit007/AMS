-- Script: 004_CreateLeaveTable
-- Description: Create Leave table for leave request tracking
-- Author: AMS Team
-- Date: 2024-03-08

CREATE TABLE IF NOT EXISTS leave (
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

CREATE INDEX IF NOT EXISTS idx_leave_employee_id ON leave(employee_id);
CREATE INDEX IF NOT EXISTS idx_leave_status ON leave(status);
CREATE INDEX IF NOT EXISTS idx_leave_type ON leave(leave_type);
CREATE INDEX IF NOT EXISTS idx_leave_from_date ON leave(from_date);
CREATE INDEX IF NOT EXISTS idx_leave_employee_status ON leave(employee_id, status);
