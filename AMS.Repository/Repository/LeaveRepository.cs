using AMS.Repository.Context;
using AMS.Repository.Entities;
using AMS.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AMS.Repository.Repository
{
    public class LeaveRepository : Repository<Leave>, ILeaveRepository
    {
        public LeaveRepository(AttendanceManagementContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Leave>> GetByEmployeeAsync(int employeeId)
        {
            return await _dbSet
                .Where(l => l.EmployeeId == employeeId)
                .OrderByDescending(l => l.FromDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Leave>> GetByStatusAsync(string status)
        {
            return await _dbSet
                .Where(l => l.Status == status)
                .OrderByDescending(l => l.FromDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Leave>> GetByEmployeeAndStatusAsync(int employeeId, string status)
        {
            return await _dbSet
                .Where(l => l.EmployeeId == employeeId && l.Status == status)
                .OrderByDescending(l => l.FromDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Leave>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _dbSet
                .Where(l => l.FromDate >= fromDate && l.ToDate <= toDate)
                .OrderByDescending(l => l.FromDate)
                .ToListAsync();
        }

        public async Task<int> GetApprovedLeaveCountByEmployeeAsync(int employeeId, string leaveType)
        {
            return await _dbSet
                .Where(l => l.EmployeeId == employeeId && l.LeaveType == leaveType && l.Status == "Approved")
                .SumAsync(l => l.NumberOfDays);
        }
    }
}
