using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsProjectGroupStaffPlanApiClient
    {
        Task<ApiResponseDto<List<ProjectGroupStaffPlanViewDto>>> GetPagedAsync(QueryParameters<string> query);
    }
}
