using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;

namespace Apha.FPSApps.Application.Services.PACT
{
    public class ProfitCentreService : IProfitCentreService
    {
        private readonly IPactApiClient _pactApiClient;

        public ProfitCentreService(IPactApiClient pactApiClient)
        {
            _pactApiClient = pactApiClient;
        }

        public async Task<ApiResponseDto<IEnumerable<ProfitCentreSettingsDto>>> GetAllProfitCentresAsync()
        {
            return await _pactApiClient.PactProfitCentre.GetAllProfitCentresAsync();
        }

        public async Task<ApiResponseDto<ProfitCentreSettingsDto>> GetProfitCentreSettingsAsync(string profitCentre)
        {
            return await _pactApiClient.PactProfitCentre.GetProfitCentreSettingsAsync(profitCentre);
        }

        public async Task<ApiResponseDto<bool>> UpdateProfitCentreSettingsAsync(
            string profitCentre, int timesheet, int outputsheet, short timesheetLayout)
        {
            return await _pactApiClient.PactProfitCentre.UpdateProfitCentreSettingsAsync(
                profitCentre, timesheet, outputsheet, timesheetLayout);
        }
    }
}
