using AMS.API.DTOs;

namespace AMS.API.Interfaces
{
    public interface IReportApiController
    {
        Task<ApiResponse<AttendanceReportDto>> GetEmployeeAttendanceReport(int employeeId, DateTime fromDate, DateTime toDate);
        Task<ApiResponse<DepartmentAttendanceReportDto>> GetDepartmentAttendanceReport(int departmentId, DateTime fromDate, DateTime toDate);
        Task<ApiResponse<PaginatedResponse<AttendanceReportDto>>> GetAllAttendanceReports(DateTime fromDate, DateTime toDate, int pageNumber = 1, int pageSize = 10);
        Task<ApiResponse<LeaveBalanceDto>> GetEmployeeLeaveBalance(int employeeId);
        Task<ApiResponse<List<LeaveBalanceDto>>> GetDepartmentLeaveBalance(int departmentId);
        Task<ApiResponse<byte[]>> ExportAttendanceReport(int employeeId, DateTime fromDate, DateTime toDate, string format = "pdf");
    }
}
