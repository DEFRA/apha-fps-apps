using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class ProjectStaffPlanActualService : IProjectStaffPlanActualService
    {
        private readonly IFpsApiClient _fpsClient;

        public ProjectStaffPlanActualService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        public async Task<ApiResponseDto<List<TimeCostCalcsViewDto>>> GetTimeCostCalcsByProjectAsync(QueryParameters<string> query, string projectCode)
            => await _fpsClient.FpsProjectStaffPlanActual.GetTimeCostCalcsByProjectAsync(query, projectCode);

        public async Task<ApiResponseDto<TimeCostCalcsTotalsDto>> GetTotalActualByProjectAsync(string projectCode)
            => await _fpsClient.FpsProjectStaffPlanActual.GetTotalActualByProjectAsync(projectCode);

        public async Task<ApiResponseDto<bool>> DeleteTimeCostCalcsAsync(string workgroup, string jobCode, string project, double month, string staffId)
            => await _fpsClient.FpsProjectStaffPlanActual.DeleteTimeCostCalcsAsync(workgroup, jobCode, project, month, staffId);
    }
}
