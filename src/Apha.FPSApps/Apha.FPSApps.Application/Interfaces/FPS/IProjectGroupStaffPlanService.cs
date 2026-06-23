using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IProjectGroupStaffPlanService
    {
        Task<ApiResponseDto<List<ProjectGroupStaffPlanViewDto>>> GetPagedAsync(QueryParameters<string> query);
    }
}
