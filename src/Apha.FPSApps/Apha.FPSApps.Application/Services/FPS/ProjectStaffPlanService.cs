using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class ProjectStaffPlanService : IProjectStaffPlanService
    {
        private readonly IFpsApiClient _fpsClient;

        public ProjectStaffPlanService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        public async Task<ApiResponseDto<List<ProjectStaffPlanViewDto>>> GetPagedAsync(QueryParameters<string> query)
            => await _fpsClient.FpsProjectStaffPlan.GetPagedAsync(query);
    }
}
