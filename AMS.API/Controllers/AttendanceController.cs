using AMS.Contracts.DTOs;
using AMS.API.Interfaces;
using AMS.Command.Commands.Attendance;
using AMS.Query.Queries.Attendance;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AttendanceController : ControllerBase, IAttendanceApiController
    {
        private readonly ILogger<AttendanceController> _logger;
        private readonly IMediator _mediator;

        public AttendanceController(ILogger<AttendanceController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpPost("mark")]
        public async Task<ApiResponse<AttendanceDto>> MarkAttendance(CreateAttendanceDto dto)
        {
            try
            {
                _logger.LogInformation("Marking attendance for employee {EmployeeId} on {Date}", dto.EmployeeId, dto.Date);
                
                var command = new MarkAttendanceCommand
                {
                    EmployeeId = dto.EmployeeId,
                    Date = dto.Date,
                    CheckInTime = dto.CheckInTime,
                    CheckOutTime = dto.CheckOutTime,
                    Status = dto.Status,
                    Remarks = dto.Remarks
                };

                var result = await _mediator.Send(command);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking attendance for employee {EmployeeId}", dto.EmployeeId);
                return new ApiResponse<AttendanceDto>(false, "Error marking attendance", new List<string> { ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ApiResponse<AttendanceDto>> GetAttendance(int id)
        {
            try
            {
                _logger.LogInformation("Getting attendance record {Id}", id);
                
                var query = new GetAttendanceQuery { Id = id };
                var result = await _mediator.Send(query);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attendance {Id}", id);
                return new ApiResponse<AttendanceDto>(false, "Error retrieving attendance", new List<string> { ex.Message });
            }
        }

        [HttpGet("employee/{employeeId}")]
        public async Task<ApiResponse<PaginatedResponse<AttendanceDto>>> GetAttendanceByEmployee(int employeeId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Getting attendance for employee {EmployeeId}, page {PageNumber}", employeeId, pageNumber);
                
                var query = new GetAttendanceByEmployeeQuery
                {
                    EmployeeId = employeeId,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                var result = await _mediator.Send(query);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attendance for employee {EmployeeId}", employeeId);
                return new ApiResponse<PaginatedResponse<AttendanceDto>>(false, "Error retrieving attendance", new List<string> { ex.Message });
            }
        }

        [HttpGet("range")]
        public async Task<ApiResponse<PaginatedResponse<AttendanceDto>>> GetAttendanceByDateRange(DateTime fromDate, DateTime toDate, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Getting attendance from {FromDate} to {ToDate}", fromDate, toDate);
                
                var query = new GetAttendanceByDateRangeQuery
                {
                    FromDate = fromDate,
                    ToDate = toDate,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                var result = await _mediator.Send(query);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attendance by date range");
                return new ApiResponse<PaginatedResponse<AttendanceDto>>(false, "Error retrieving attendance", new List<string> { ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ApiResponse<AttendanceDto>> UpdateAttendance(int id, UpdateAttendanceDto dto)
        {
            try
            {
                _logger.LogInformation("Updating attendance {Id}", id);
                
                var command = new UpdateAttendanceCommand
                {
                    Id = id,
                    CheckOutTime = dto.CheckOutTime,
                    Status = dto.Status,
                    Remarks = dto.Remarks
                };

                var result = await _mediator.Send(command);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating attendance {Id}", id);
                return new ApiResponse<AttendanceDto>(false, "Error updating attendance", new List<string> { ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ApiResponse<bool>> DeleteAttendance(int id)
        {
            try
            {
                _logger.LogInformation("Deleting attendance {Id}", id);
                
                var command = new DeleteAttendanceCommand { Id = id };
                var result = await _mediator.Send(command);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting attendance {Id}", id);
                return new ApiResponse<bool>(false, "Error deleting attendance", new List<string> { ex.Message });
            }
        }
    }
}
