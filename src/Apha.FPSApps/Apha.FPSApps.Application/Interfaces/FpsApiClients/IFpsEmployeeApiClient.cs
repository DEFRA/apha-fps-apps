using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsEmployeeApiClient
    {
        Task<ApiResponseDto<List<EmployeeDto>>> GetFilteredEmployeesAsync(QueryParameters<string> criteria, int filterOption);
        Task<ApiResponseDto<EmployeeDto>> GetEmployeeIdAsync(string spNumber);
        Task<ApiResponseDto<EmployeeDto>> CreateEmployeeAsync(EmployeeDto employee);
        Task<ApiResponseDto<EmployeeDto>> UpdateEmployeeAsync(EmployeeDto employee);
        Task<ApiResponseDto<bool>> DeleteEmployeeAsync(string spNumber);
        Task<ApiResponseDto<List<ManagerDto>>> GetAllManagerAsync();
        Task<ApiResponseDto<List<ManagerDto>>> GetAllPactManagerAsync();
        Task<ApiResponseDto<List<WorkGroupPersonDto>>> GetAllWorkGroupPersonAsync();
        Task<ApiResponseDto<List<WorkGroupStaffDto>>> GetActiveStaffAsync();
        Task<ApiResponseDto<PaginatedResult<WorkGroupStaffDto>>> GetWorkGroupStaffAsync(QueryParameters<string> query, string? workGroup = null);
    }
}
