using AMS.API.DTOs;
using AMS.Query.Queries.Attendance;
using RepoAttendance = AMS.Repository.Entities.Attendance;
using AMS.Repository.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AMS.Query.Handlers
{
    public class GetAttendanceByDateRangeQueryHandler : IRequestHandler<GetAttendanceByDateRangeQuery, ApiResponse<PaginatedResponse<AttendanceDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetAttendanceByDateRangeQueryHandler> _logger;

        public GetAttendanceByDateRangeQueryHandler(IUnitOfWork unitOfWork, ILogger<GetAttendanceByDateRangeQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<PaginatedResponse<AttendanceDto>>> Handle(GetAttendanceByDateRangeQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Validate date range
                if (request.FromDate > request.ToDate)
                {
                    _logger.LogWarning("Invalid date range: FromDate {FromDate} is greater than ToDate {ToDate}", request.FromDate, request.ToDate);
                    return new ApiResponse<PaginatedResponse<AttendanceDto>>(false, "FromDate must be less than or equal to ToDate");
                }

                _logger.LogInformation("Fetching attendance from {FromDate} to {ToDate}, page {PageNumber}, size {PageSize}", 
                    request.FromDate, request.ToDate, request.PageNumber, request.PageSize);

                var attendanceRecords = await _unitOfWork.Attendances.GetByDateRangeAsync(request.FromDate, request.ToDate);
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
                _logger.LogError(ex, "Error retrieving attendance by date range");
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
