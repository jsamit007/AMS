using AMS.Repository.Context;
using AMS.Repository.Interfaces;
using AMS.Repository.Repository;

namespace AMS.Repository.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IEmployeeRepository Employees { get; }
        IAttendanceRepository Attendances { get; }
        IDepartmentRepository Departments { get; }
        ILeaveRepository Leaves { get; }

        Task<bool> SaveChangesAsync();
    }

    public class UnitOfWork : IUnitOfWork
    {
        private readonly AttendanceManagementContext _context;
        private IEmployeeRepository? _employeeRepository;
        private IAttendanceRepository? _attendanceRepository;
        private IDepartmentRepository? _departmentRepository;
        private ILeaveRepository? _leaveRepository;

        public UnitOfWork(AttendanceManagementContext context)
        {
            _context = context;
        }

        public IEmployeeRepository Employees
        {
            get
            {
                _employeeRepository ??= new EmployeeRepository(_context);
                return _employeeRepository;
            }
        }

        public IAttendanceRepository Attendances
        {
            get
            {
                _attendanceRepository ??= new AttendanceRepository(_context);
                return _attendanceRepository;
            }
        }

        public IDepartmentRepository Departments
        {
            get
            {
                _departmentRepository ??= new DepartmentRepository(_context);
                return _departmentRepository;
            }
        }

        public ILeaveRepository Leaves
        {
            get
            {
                _leaveRepository ??= new LeaveRepository(_context);
                return _leaveRepository;
            }
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
