using AMS.API.DTOs;
using AMS.Query.Queries.Attendance;
using RepoAttendance = AMS.Repository.Entities.Attendance;
using AMS.Repository.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AMS.Query.Handlers
{
    public class GetAttendanceByEmployeeQueryHandler : IRequestHandler<GetAttendanceByEmployeeQuery, ApiResponse<PaginatedResponse<AttendanceDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetAttendanceByEmployeeQueryHandler> _logger;

        public GetAttendanceByEmployeeQueryHandler(IUnitOfWork unitOfWork, ILogger<GetAttendanceByEmployeeQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<PaginatedResponse<AttendanceDto>>> Handle(GetAttendanceByEmployeeQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching attendance for employee {EmployeeId}, page {PageNumber}, size {PageSize}", 
                    request.EmployeeId, request.PageNumber, request.PageSize);

                // Verify employee exists
                var employee = await _unitOfWork.Employees.GetByIdAsync(request.EmployeeId);
                if (employee == null)
                {
                    _logger.LogWarning("Employee {EmployeeId} not found", request.EmployeeId);
                    return new ApiResponse<PaginatedResponse<AttendanceDto>>(false, $"Employee with ID {request.EmployeeId} not found");
                }

                // Get paginated attendance records
                var attendanceRecords = await _unitOfWork.Attendances.GetByEmployeeAsync(request.EmployeeId);
                var totalRecords = attendanceRecords.Count();

                var pagedRecords = attendanceRecords
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                var dtos = pagedRecords.Select(MapToDto).ToList();
                var response = new PaginatedResponse<AttendanceDto>(dtos, request.PageNumber, request.PageSize, totalRecords);

                return new ApiResponse<PaginatedResponse<AttendanceDto>>(true, "Attendance retrieved successfully", response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attendance for employee {EmployeeId}", request.EmployeeId);
                return new ApiResponse<PaginatedResponse<AttendanceDto>>(false, "Error retrieving attendance", new List<string> { ex.Message });
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
