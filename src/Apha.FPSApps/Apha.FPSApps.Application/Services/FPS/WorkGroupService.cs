using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class WorkGroupService : IWorkGroupService
    {
        private readonly IFpsApiClient _fpsClient;

        public WorkGroupService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        public async Task<ApiResponseDto<List<WorkGroupViewDto>>> GetWorkGroupsAsync(string profitCentre)
        {
            return await _fpsClient.FpsWorkGroup.GetWorkGroupsAsync(profitCentre);
        }
    }
}
