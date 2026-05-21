using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;

namespace Apha.FPSApps.Application.Interfaces.PactApiClients
{
    public interface IPactProfitCentreApiClient
    {
        Task<ApiResponseDto<IEnumerable<ProfitCentreSettingsDto>>> GetAllProfitCentresAsync();
        Task<ApiResponseDto<ProfitCentreSettingsDto>> GetProfitCentreSettingsAsync(string profitCentre);
        Task<ApiResponseDto<bool>> UpdateProfitCentreSettingsAsync(string profitCentre, int timesheet, int outputsheet, short timesheetLayout);
    }
}
