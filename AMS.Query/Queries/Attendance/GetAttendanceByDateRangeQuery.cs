using AMS.Contracts.DTOs;
using MediatR;

namespace AMS.Query.Queries.Attendance
{
    public class GetAttendanceByDateRangeQuery : IRequest<ApiResponse<PaginatedResponse<AttendanceDto>>>
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
