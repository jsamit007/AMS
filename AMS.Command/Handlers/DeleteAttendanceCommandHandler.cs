using AMS.API.DTOs;
using AMS.Command.Commands.Attendance;
using AMS.Repository.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AMS.Command.Handlers
{
    public class DeleteAttendanceCommandHandler : IRequestHandler<DeleteAttendanceCommand, ApiResponse<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteAttendanceCommandHandler> _logger;

        public DeleteAttendanceCommandHandler(IUnitOfWork unitOfWork, ILogger<DeleteAttendanceCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<bool>> Handle(DeleteAttendanceCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Deleting attendance {Id}", request.Id);

                var attendance = await _unitOfWork.Attendances.GetByIdAsync(request.Id);
                if (attendance == null)
                {
                    _logger.LogWarning("Attendance {Id} not found", request.Id);
                    return new ApiResponse<bool>(false, $"Attendance with ID {request.Id} not found");
                }

                var deleted = await _unitOfWork.Attendances.DeleteAsync(attendance);
                if (!deleted)
                {
                    _logger.LogError("Failed to delete attendance {Id}", request.Id);
                    return new ApiResponse<bool>(false, "Failed to delete attendance");
                }

                var saved = await _unitOfWork.SaveChangesAsync();
                if (!saved)
                {
                    _logger.LogError("Failed to save changes after deleting attendance {Id}", request.Id);
                    return new ApiResponse<bool>(false, "Failed to delete attendance");
                }

                return new ApiResponse<bool>(true, "Attendance deleted successfully", true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting attendance {Id}", request.Id);
                return new ApiResponse<bool>(false, "Error deleting attendance", new List<string> { ex.Message });
            }
        }
    }
}
