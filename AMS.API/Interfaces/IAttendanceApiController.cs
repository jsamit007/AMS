using AMS.API.DTOs;

namespace AMS.API.Interfaces
{
    public interface IAttendanceApiController
    {
        Task<ApiResponse<AttendanceDto>> MarkAttendance(CreateAttendanceDto dto);
        Task<ApiResponse<AttendanceDto>> GetAttendance(int id);
        Task<ApiResponse<PaginatedResponse<AttendanceDto>>> GetAttendanceByEmployee(int employeeId, int pageNumber = 1, int pageSize = 10);
        Task<ApiResponse<PaginatedResponse<AttendanceDto>>> GetAttendanceByDateRange(DateTime fromDate, DateTime toDate, int pageNumber = 1, int pageSize = 10);
        Task<ApiResponse<AttendanceDto>> UpdateAttendance(int id, UpdateAttendanceDto dto);
        Task<ApiResponse<bool>> DeleteAttendance(int id);
    }
}
