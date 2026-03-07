using AMS.Contracts.DTOs;
using MediatR;

namespace AMS.Command.Commands.Attendance
{
    public class DeleteAttendanceCommand : IRequest<ApiResponse<bool>>
    {
        public int Id { get; set; }
    }
}
