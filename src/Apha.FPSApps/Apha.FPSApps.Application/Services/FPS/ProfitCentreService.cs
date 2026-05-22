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

        public async Task<ApiResponseDto<IEnumerable<ProfitCentreDto>>> GetAllProfitCentresAsync()
        {
            return await _fpsClient.FpsProfitCentre.GetAllProfitCentresAsync();
        }

        public async Task<ApiResponseDto<ProfitCentreDto>> GetProfitCentreByIdAsync(string profitCentre)
        {
            return await _fpsClient.FpsProfitCentre.GetProfitCentreByIdAsync(profitCentre);
        }

        public async Task<ApiResponseDto<bool>> UpdateProfitCentreSettingsAsync(
            string profitCentre, int timesheet, int outputsheet, short timesheetLayout)
        {
            return await _fpsClient.FpsProfitCentre.UpdateProfitCentreSettingsAsync(
                profitCentre, timesheet, outputsheet, timesheetLayout);
        }
    }
}
