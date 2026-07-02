using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class TotalBusinessOverheadsService : ITotalBusinessOverheadsService
    {
        private readonly IFpsApiClient _fpsClient;

        public TotalBusinessOverheadsService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        public async Task<ApiResponseDto<TotalBusinessOverheadsDto>> GetAsync()
        {
            return await _fpsClient.FpsTotalBusinessOverheads.GetAsync();
        }

        public async Task<ApiResponseDto<TotalBusinessOverheadsDto>> UpdateAsync(TotalBusinessOverheadsDto dto)
        {
            return await _fpsClient.FpsTotalBusinessOverheads.UpdateAsync(dto);
        }
    }
}
