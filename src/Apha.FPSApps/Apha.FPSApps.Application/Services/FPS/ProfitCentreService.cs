using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class ProfitCentreService : IProfitCentreService
    {
        private readonly IFpsApiClient _fpsClient;

        public ProfitCentreService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        public async Task<ApiResponseDto<List<ProfitCentreDto>>> GetProfitCentresAsync()
        {
            return await _fpsClient.FpsProfitCentre.GetProfitCentresAsync();
        }
    }
}
