using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;

namespace Apha.FPSApps.Application.Interfaces.PACT
{
    public interface IProjectMonthService
    {
        Task<ApiResponseDto<List<MonthDto>>> GetMonthsAsync();
        Task<ApiResponseDto<List<ProjectMonthDto>>> GetProjectMonthByProjectAsync(string project);
        Task<ApiResponseDto<ProjectMonthDto>> GetProjectMonthAsync(string project, int monthNo);
        Task<ApiResponseDto<ProjectMonthDto>> CreateProjectMonthAsync(ProjectMonthDto dto);
        Task<ApiResponseDto<ProjectMonthDto>> UpdateProjectMonthAsync(ProjectMonthDto dto);
        Task<ApiResponseDto<bool>> DeleteProjectMonthAsync(string project, int monthNo);
    }
}
