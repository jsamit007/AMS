using AMS.Repository.Context;
using AMS.Repository.Entities;
using AMS.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AMS.Repository.Repository
{
    public class DepartmentRepository : Repository<Department>, IDepartmentRepository
    {
        public DepartmentRepository(AttendanceManagementContext context) : base(context)
        {
        }

        public async Task<Department?> GetByCodeAsync(string code)
        {
            return await _dbSet.FirstOrDefaultAsync(d => d.Code == code);
        }

        public async Task<Department?> GetWithEmployeesAsync(int id)
        {
            return await _dbSet
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<IEnumerable<Department>> GetActiveAsync()
        {
            return await _dbSet
                .Where(d => d.IsActive)
                .ToListAsync();
        }
    }
}
