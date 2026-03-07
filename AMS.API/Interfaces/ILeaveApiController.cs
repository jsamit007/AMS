using AMS.API.DTOs;

namespace AMS.API.Interfaces
{
    public interface ILeaveApiController
    {
        Task<ApiResponse<LeaveDto>> GetLeave(int id);
        Task<ApiResponse<PaginatedResponse<LeaveDto>>> GetLeaveByEmployee(int employeeId, int pageNumber = 1, int pageSize = 10);
        Task<ApiResponse<PaginatedResponse<LeaveDto>>> GetLeaveByStatus(string status, int pageNumber = 1, int pageSize = 10);
        Task<ApiResponse<LeaveDto>> ApplyLeave(CreateLeaveDto dto);
        Task<ApiResponse<LeaveDto>> ApproveLeave(int id, int approvedBy);
        Task<ApiResponse<LeaveDto>> RejectLeave(int id, int approvedBy);
        Task<ApiResponse<bool>> DeleteLeave(int id);
        Task<ApiResponse<List<LeaveBalanceDto>>> GetLeaveBalance(int employeeId);
    }
}
