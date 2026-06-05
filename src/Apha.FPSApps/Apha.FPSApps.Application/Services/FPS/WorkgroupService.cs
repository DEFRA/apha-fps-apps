using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class WorkgroupService : IWorkgroupService
    {
        private readonly IFpsApiClient _fpsClient;

        public WorkgroupService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient ?? throw new ArgumentNullException(nameof(fpsClient));
        }

        public async Task<ApiResponseDto<List<string>>> GetAllWorkgroupNamesAsync()
            => await _fpsClient.FpsWorkgroup.GetAllWorkgroupNamesAsync();
    }
}
