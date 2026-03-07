using AMS.Repository.Context;
using AMS.Repository.Entities;
using AMS.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AMS.Repository.Repository
{
    public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(AttendanceManagementContext context) : base(context)
        {
        }

        public async Task<Employee?> GetByCodeAsync(string code)
        {
            return await _dbSet.FirstOrDefaultAsync(e => e.EmployeeCode == code);
        }

        public async Task<Employee?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(e => e.Email == email);
        }

        public async Task<IEnumerable<Employee>> GetByDepartmentAsync(int departmentId)
        {
            return await _dbSet
                .Where(e => e.DepartmentId == departmentId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Employee>> GetActiveEmployeesAsync()
        {
            return await _dbSet
                .Where(e => e.IsActive)
                .ToListAsync();
        }
    }
}
