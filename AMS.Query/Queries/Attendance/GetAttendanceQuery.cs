using AMS.Contracts.DTOs;
using MediatR;

namespace AMS.Query.Queries.Attendance
{
    public class GetAttendanceQuery : IRequest<ApiResponse<AttendanceDto>>
    {
        public int Id { get; set; }
    }
}
