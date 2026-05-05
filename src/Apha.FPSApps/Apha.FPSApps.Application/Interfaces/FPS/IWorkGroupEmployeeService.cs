using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IWorkGroupEmployeeService
    {
        Task<ApiResponseDto<List<WgEmployeeViewDto>>> GetWorkGroupEmployeeAsync(QueryParameters<string> query, string wgGrade);
        Task<ApiResponseDto<WgEmployeeDto>> GetWorkGroupEmployeeByIdAsync(string pactId);
        Task<ApiResponseDto<WgEmployeeDto>> UpdateWorkGroupEmployeeAsync(WgEmployeeDto dto);
        Task<ApiResponseDto<bool>> DeleteWorkGroupEmployeeAsync(string pactId);
    }
}
