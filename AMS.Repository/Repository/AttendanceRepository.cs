using AMS.Repository.Context;
using AMS.Repository.Entities;
using AMS.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AMS.Repository.Repository
{
    public class AttendanceRepository : Repository<Attendance>, IAttendanceRepository
    {
        public AttendanceRepository(AttendanceManagementContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Attendance>> GetByEmployeeAsync(int employeeId)
        {
            return await _dbSet
                .Where(a => a.EmployeeId == employeeId)
                .OrderByDescending(a => a.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Attendance>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _dbSet
                .Where(a => a.Date >= fromDate && a.Date <= toDate)
                .OrderByDescending(a => a.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Attendance>> GetByEmployeeAndDateRangeAsync(int employeeId, DateTime fromDate, DateTime toDate)
        {
            return await _dbSet
                .Where(a => a.EmployeeId == employeeId && a.Date >= fromDate && a.Date <= toDate)
                .OrderByDescending(a => a.Date)
                .ToListAsync();
        }

        public async Task<Attendance?> GetByEmployeeAndDateAsync(int employeeId, DateTime date)
        {
            return await _dbSet
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date == date.Date);
        }
    }
}
