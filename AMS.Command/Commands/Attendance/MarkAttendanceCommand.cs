using AMS.API.DTOs;
using MediatR;

namespace AMS.Command.Commands.Attendance
{
    public class MarkAttendanceCommand : IRequest<ApiResponse<AttendanceDto>>
    {
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan CheckInTime { get; set; }
        public TimeSpan? CheckOutTime { get; set; }
        public string Status { get; set; } = null!; // Present, Absent, Late, LeaveApproved
        public string? Remarks { get; set; }
    }
}
