using AMS.API.DTOs;
using AMS.Command.Commands.Attendance;
using RepoAttendance = AMS.Repository.Entities.Attendance;
using AMS.Repository.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AMS.Command.Handlers
{
    public class MarkAttendanceCommandHandler : IRequestHandler<MarkAttendanceCommand, ApiResponse<AttendanceDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<MarkAttendanceCommandHandler> _logger;

        public MarkAttendanceCommandHandler(IUnitOfWork unitOfWork, ILogger<MarkAttendanceCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<AttendanceDto>> Handle(MarkAttendanceCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Marking attendance for employee {EmployeeId} on {Date}", request.EmployeeId, request.Date);

                // Verify employee exists
                var employee = await _unitOfWork.Employees.GetByIdAsync(request.EmployeeId);
                if (employee == null)
                {
                    _logger.LogWarning("Employee {EmployeeId} not found", request.EmployeeId);
                    return new ApiResponse<AttendanceDto>(false, $"Employee with ID {request.EmployeeId} not found");
                }

                // Check if attendance already exists for this date
                var existingAttendance = await _unitOfWork.Attendances.GetByEmployeeAndDateAsync(request.EmployeeId, request.Date);
                if (existingAttendance != null)
                {
                    _logger.LogWarning("Attendance already marked for employee {EmployeeId} on {Date}", request.EmployeeId, request.Date);
                    return new ApiResponse<AttendanceDto>(false, "Attendance already marked for this date");
                }

                // Create new attendance record
                var attendance = new RepoAttendance
                {
                    EmployeeId = request.EmployeeId,
                    Date = request.Date,
                    CheckInTime = request.CheckInTime,
                    CheckOutTime = request.CheckOutTime,
                    Status = request.Status,
                    Remarks = request.Remarks,
                    CreatedDate = DateTime.UtcNow
                };

                await _unitOfWork.Attendances.AddAsync(attendance);
                var saved = await _unitOfWork.SaveChangesAsync();

                if (!saved)
                {
                    _logger.LogError("Failed to save attendance for employee {EmployeeId}", request.EmployeeId);
                    return new ApiResponse<AttendanceDto>(false, "Failed to mark attendance");
                }

                var dto = MapToDto(attendance);
                return new ApiResponse<AttendanceDto>(true, "Attendance marked successfully", dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking attendance for employee {EmployeeId}", request.EmployeeId);
                return new ApiResponse<AttendanceDto>(false, "Error marking attendance", new List<string> { ex.Message });
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
