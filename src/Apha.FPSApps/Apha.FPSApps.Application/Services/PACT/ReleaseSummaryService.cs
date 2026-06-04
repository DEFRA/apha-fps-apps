using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;

namespace Apha.FPSApps.Application.Services.PACT
{
    public class ReleaseSummaryService : IReleaseSummaryService
    {
        private readonly IPactApiClient _pactClient;

        public ReleaseSummaryService(IPactApiClient pactClient)
        {
            _pactClient = pactClient;
        }

        public async Task<ApiResponseDto<ReleaseSummaryDto>> GetReleaseSummariesAsync()
        {
            return await _pactClient.PactReleaseSummary.GetReleaseSummariesAsync();
        }

        public async Task<ApiResponseDto<ReleasePeriodDto>> SetFinalSummaryRunAsync(string? periodName, short? finalSummariesRun, string? sendEmail)
        {
            return await _pactClient.PactReleaseSummary.SetFinalSummaryRunAsync(periodName, finalSummariesRun, sendEmail);
        }
    }
}