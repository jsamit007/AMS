using AMS.Contracts.DTOs;
using AMS.Query.Queries.Attendance;
using RepoAttendance = AMS.Repository.Entities.Attendance;
using AMS.Repository.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AMS.Query.Handlers
{
    public class GetAttendanceQueryHandler : IRequestHandler<GetAttendanceQuery, ApiResponse<AttendanceDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetAttendanceQueryHandler> _logger;

        public GetAttendanceQueryHandler(IUnitOfWork unitOfWork, ILogger<GetAttendanceQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<AttendanceDto>> Handle(GetAttendanceQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching attendance {Id}", request.Id);

                var attendance = await _unitOfWork.Attendances.GetByIdAsync(request.Id);
                if (attendance == null)
                {
                    _logger.LogWarning("Attendance {Id} not found", request.Id);
                    return new ApiResponse<AttendanceDto>(false, $"Attendance with ID {request.Id} not found");
                }

                var dto = MapToDto(attendance);
                return new ApiResponse<AttendanceDto>(true, "Attendance retrieved successfully", dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attendance {Id}", request.Id);
                return new ApiResponse<AttendanceDto>(false, "Error retrieving attendance", new List<string> { ex.Message });
            }
        }

        private AttendanceDto MapToDto(RepoAttendance attendance)
        {
            return new AttendanceDto
            {
                Id = attendance.Id,
                EmployeeId = attendance.EmployeeId,
                Date = attendance.Date,
                CheckInTime = attendance.CheckInTime,
                CheckOutTime = attendance.CheckOutTime,
                Status = attendance.Status,
                Remarks = attendance.Remarks
            };
        }
    }
}
