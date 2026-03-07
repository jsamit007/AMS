-- Script: 002_CreateEmployeeTable
-- Description: Create Employee table with foreign key to Department
-- Author: AMS Team
-- Date: 2024-03-08

CREATE TABLE IF NOT EXISTS employee (
    id SERIAL PRIMARY KEY,
    employee_code VARCHAR(20) NOT NULL UNIQUE,
    first_name VARCHAR(50) NOT NULL,
    last_name VARCHAR(50) NOT NULL,
    email VARCHAR(100) NOT NULL UNIQUE,
    phone_number VARCHAR(20) NOT NULL,
    department_id INTEGER NOT NULL,
    designation VARCHAR(100) NOT NULL,
    joining_date TIMESTAMP NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_date TIMESTAMP,
    FOREIGN KEY (department_id) REFERENCES department(id)
);

CREATE INDEX IF NOT EXISTS idx_employee_code ON employee(employee_code);
CREATE INDEX IF NOT EXISTS idx_employee_email ON employee(email);
CREATE INDEX IF NOT EXISTS idx_employee_department_id ON employee(department_id);
CREATE INDEX IF NOT EXISTS idx_employee_is_active ON employee(is_active);
