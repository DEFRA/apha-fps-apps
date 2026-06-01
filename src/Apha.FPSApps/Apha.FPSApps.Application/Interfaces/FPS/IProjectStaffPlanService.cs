using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IProjectStaffPlanService
    {
        Task<ApiResponseDto<List<ProjectStaffPlanViewDto>>> GetPagedAsync(QueryParameters<string> query);
    }
}
