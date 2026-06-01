using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsProjectStaffPlanApiClient
    {
        Task<ApiResponseDto<List<ProjectStaffPlanViewDto>>> GetPagedAsync(QueryParameters<string> query);
    }
}
