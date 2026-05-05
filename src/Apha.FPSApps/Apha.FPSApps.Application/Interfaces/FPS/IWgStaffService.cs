using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IWgStaffService
    {
        Task<ApiResponseDto<List<WgEmployeeViewDto>>> GetWgStaffAsync(QueryParameters<string> query, string wgGrade);
        Task<ApiResponseDto<WgEmployeeDto>> GetWgEmployeeByIdAsync(string pactId);
        Task<ApiResponseDto<WgEmployeeDto>> UpdateWgEmployeeAsync(WgEmployeeDto dto);
        Task<ApiResponseDto<bool>> DeleteWgEmployeeAsync(string pactId);
    }
}
