using AMS.Repository.Entities;

namespace AMS.Repository.Interfaces
{
    public interface IRepository<T> where T : class
    {
        // Read operations
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize);
        Task<int> GetTotalCountAsync();

        // Create operations
        Task<T> AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);

        // Update operations
        Task<T> UpdateAsync(T entity);

        // Delete operations
        Task<bool> DeleteAsync(int id);
        Task<bool> DeleteAsync(T entity);
        Task<bool> DeleteRangeAsync(IEnumerable<T> entities);

        // Save changes
        Task<bool> SaveChangesAsync();
    }

    public interface IEmployeeRepository : IRepository<Employee>
    {
        Task<Employee?> GetByCodeAsync(string code);
        Task<Employee?> GetByEmailAsync(string email);
        Task<IEnumerable<Employee>> GetByDepartmentAsync(int departmentId);
        Task<IEnumerable<Employee>> GetActiveEmployeesAsync();
    }

    public interface IAttendanceRepository : IRepository<Attendance>
    {
        Task<IEnumerable<Attendance>> GetByEmployeeAsync(int employeeId);
        Task<IEnumerable<Attendance>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<IEnumerable<Attendance>> GetByEmployeeAndDateRangeAsync(int employeeId, DateTime fromDate, DateTime toDate);
        Task<Attendance?> GetByEmployeeAndDateAsync(int employeeId, DateTime date);
    }

    public interface IDepartmentRepository : IRepository<Department>
    {
        Task<Department?> GetByCodeAsync(string code);
        Task<Department?> GetWithEmployeesAsync(int id);
        Task<IEnumerable<Department>> GetActiveAsync();
    }

    public interface ILeaveRepository : IRepository<Leave>
    {
        Task<IEnumerable<Leave>> GetByEmployeeAsync(int employeeId);
        Task<IEnumerable<Leave>> GetByStatusAsync(string status);
        Task<IEnumerable<Leave>> GetByEmployeeAndStatusAsync(int employeeId, string status);
        Task<IEnumerable<Leave>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<int> GetApprovedLeaveCountByEmployeeAsync(int employeeId, string leaveType);
    }
}
