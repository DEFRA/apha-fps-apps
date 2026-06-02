using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;

namespace Apha.FPSApps.Application.Interfaces.PactApiClients
{
    public interface IPactReleaseSummaryApiClient
    {
        Task<ApiResponseDto<IReadOnlyList<ReleasePeriodDto>>> GetReleaseSummariesAsync();
        Task<ApiResponseDto<short>> SetFinalSummaryRunAsync(string periodName, short finalSummariesRun);
    }
}