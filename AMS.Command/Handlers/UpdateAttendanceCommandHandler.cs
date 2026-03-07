using AMS.API.DTOs;
using AMS.Command.Commands.Attendance;
using RepoAttendance = AMS.Repository.Entities.Attendance;
using AMS.Repository.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AMS.Command.Handlers
{
    public class UpdateAttendanceCommandHandler : IRequestHandler<UpdateAttendanceCommand, ApiResponse<AttendanceDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateAttendanceCommandHandler> _logger;

        public UpdateAttendanceCommandHandler(IUnitOfWork unitOfWork, ILogger<UpdateAttendanceCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<AttendanceDto>> Handle(UpdateAttendanceCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Updating attendance {Id}", request.Id);

                var attendance = await _unitOfWork.Attendances.GetByIdAsync(request.Id);
                if (attendance == null)
                {
                    _logger.LogWarning("Attendance {Id} not found", request.Id);
                    return new ApiResponse<AttendanceDto>(false, $"Attendance with ID {request.Id} not found");
                }

                // Update only provided fields
                if (request.CheckOutTime.HasValue)
                    attendance.CheckOutTime = request.CheckOutTime;
                
                if (!string.IsNullOrEmpty(request.Status))
                    attendance.Status = request.Status;
                
                if (!string.IsNullOrEmpty(request.Remarks))
                    attendance.Remarks = request.Remarks;

                attendance.UpdatedDate = DateTime.UtcNow;

                await _unitOfWork.Attendances.UpdateAsync(attendance);
                var saved = await _unitOfWork.SaveChangesAsync();

                if (!saved)
                {
                    _logger.LogError("Failed to update attendance {Id}", request.Id);
                    return new ApiResponse<AttendanceDto>(false, "Failed to update attendance");
                }

                var dto = MapToDto(attendance);
                return new ApiResponse<AttendanceDto>(true, "Attendance updated successfully", dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating attendance {Id}", request.Id);
                return new ApiResponse<AttendanceDto>(false, "Error updating attendance", new List<string> { ex.Message });
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
