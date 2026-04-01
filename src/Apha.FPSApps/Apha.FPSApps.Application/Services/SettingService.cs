using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Interfaces;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;

namespace Apha.FPSApps.Application.Services
{
    public class SettingService : ISettingService
    {
        private readonly IFpsApiClient _fpsClient;

        public SettingService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        public async Task<ApiResponseDto<decimal>> GetHoursPerDayAsync()
        {
            return await _fpsClient.FpsSetting.GetHoursPerDayAsync();
        }
    }
}
