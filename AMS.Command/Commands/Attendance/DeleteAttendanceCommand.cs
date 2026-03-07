using AMS.API.DTOs;
using MediatR;

namespace AMS.Command.Commands.Attendance
{
    public class DeleteAttendanceCommand : IRequest<ApiResponse<bool>>
    {
        public int Id { get; set; }
    }
}
