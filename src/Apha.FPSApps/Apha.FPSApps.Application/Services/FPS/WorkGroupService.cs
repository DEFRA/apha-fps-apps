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
            _fpsClient = fpsClient ?? throw new ArgumentNullException(nameof(fpsClient));
        }

        public async Task<ApiResponseDto<List<string>>> GetAllWorkGroupNamesAsync()
            => await _fpsClient.FpsWorkGroup.GetAllWorkGroupNamesAsync();

        public async Task<ApiResponseDto<List<WorkGroupViewDto>>> GetWorkGroupsAsync(string profitCentre)
            => await _fpsClient.FpsWorkGroup.GetWorkGroupsAsync(profitCentre);
    }
}
