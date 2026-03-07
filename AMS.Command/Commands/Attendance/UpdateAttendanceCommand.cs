using AMS.API.DTOs;
using MediatR;

namespace AMS.Command.Commands.Attendance
{
    public class UpdateAttendanceCommand : IRequest<ApiResponse<AttendanceDto>>
    {
        public int Id { get; set; }
        public TimeSpan? CheckOutTime { get; set; }
        public string? Status { get; set; }
        public string? Remarks { get; set; }
    }
}
