using AMS.API.DTOs;

namespace AMS.API.Interfaces
{
    public interface IEmployeeApiController
    {
        Task<ApiResponse<EmployeeDto>> GetEmployee(int id);
        Task<ApiResponse<PaginatedResponse<EmployeeDto>>> GetAllEmployees(int pageNumber = 1, int pageSize = 10);
        Task<ApiResponse<PaginatedResponse<EmployeeDto>>> GetEmployeesByDepartment(int departmentId, int pageNumber = 1, int pageSize = 10);
        Task<ApiResponse<EmployeeDto>> CreateEmployee(CreateEmployeeDto dto);
        Task<ApiResponse<EmployeeDto>> UpdateEmployee(int id, UpdateEmployeeDto dto);
        Task<ApiResponse<bool>> DeleteEmployee(int id);
        Task<ApiResponse<EmployeeDto>> GetEmployeeByCode(string code);
    }
}
