using AMS.Contracts.DTOs;
using MediatR;

namespace AMS.Query.Queries.Attendance
{
    public class GetAttendanceByEmployeeQuery : IRequest<ApiResponse<PaginatedResponse<AttendanceDto>>>
    {
        public int EmployeeId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
