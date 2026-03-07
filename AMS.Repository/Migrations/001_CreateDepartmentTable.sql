-- Script: 001_CreateDepartmentTable
-- Description: Create Department table
-- Author: AMS Team
-- Date: 2024-03-08

CREATE TABLE IF NOT EXISTS department (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    code VARCHAR(20) NOT NULL UNIQUE,
    description VARCHAR(500),
    manager_id INTEGER,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_date TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_department_code ON department(code);
CREATE INDEX IF NOT EXISTS idx_department_is_active ON department(is_active);
