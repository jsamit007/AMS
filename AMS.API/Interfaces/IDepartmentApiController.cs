using AMS.API.DTOs;

namespace AMS.API.Interfaces
{
    public interface IDepartmentApiController
    {
        Task<ApiResponse<DepartmentDto>> GetDepartment(int id);
        Task<ApiResponse<PaginatedResponse<DepartmentDto>>> GetAllDepartments(int pageNumber = 1, int pageSize = 10);
        Task<ApiResponse<DepartmentDto>> GetDepartmentByCode(string code);
        Task<ApiResponse<DepartmentDto>> CreateDepartment(CreateDepartmentDto dto);
        Task<ApiResponse<DepartmentDto>> UpdateDepartment(int id, UpdateDepartmentDto dto);
        Task<ApiResponse<bool>> DeleteDepartment(int id);
    }
}
