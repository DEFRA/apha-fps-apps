using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class ProjPlanVsActualsStaffService : IProjPlanVsActualsStaffService
    {
        private readonly IFpsApiClient _fpsClient;

        public ProjPlanVsActualsStaffService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        public async Task<ApiResponseDto<List<TimeCostCalcsViewDto>>> GetTimeCostCalcsByProjectAsync(QueryParameters<string> query, string projectCode)
            => await _fpsClient.FpsProjPlanVsActualsStaff.GetTimeCostCalcsByProjectAsync(query, projectCode);

        public async Task<ApiResponseDto<TimeCostCalcsTotalsDto>> GetTotalActualByProjectAsync(string projectCode)
            => await _fpsClient.FpsProjPlanVsActualsStaff.GetTotalActualByProjectAsync(projectCode);

        public async Task<ApiResponseDto<bool>> DeleteTimeCostCalcsAsync(string workgroup, string jobCode, string project, double month, string staffId)
            => await _fpsClient.FpsProjPlanVsActualsStaff.DeleteTimeCostCalcsAsync(workgroup, jobCode, project, month, staffId);
    }
}
