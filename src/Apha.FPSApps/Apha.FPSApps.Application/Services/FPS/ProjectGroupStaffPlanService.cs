using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class ProjectGroupStaffPlanService : IProjectGroupStaffPlanService
    {
        private readonly IFpsApiClient _fpsClient;

        public ProjectGroupStaffPlanService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        public async Task<ApiResponseDto<List<ProjectGroupStaffPlanViewDto>>> GetPagedAsync(QueryParameters<string> query)
            => await _fpsClient.FpsProjectGroupStaffPlan.GetPagedAsync(query);
    }
}
