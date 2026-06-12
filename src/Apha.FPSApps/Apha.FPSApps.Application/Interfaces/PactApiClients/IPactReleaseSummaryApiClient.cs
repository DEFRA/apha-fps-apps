using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;

namespace Apha.FPSApps.Application.Interfaces.PactApiClients
{
    public interface IPactReleaseSummaryApiClient
    {
        Task<ApiResponseDto<ReleaseSummaryDto>> GetReleaseSummariesAsync();
        Task<ApiResponseDto<IReadOnlyList<ReleasePeriodDto>>> GetReleasePeriodsAsync();
        Task<ApiResponseDto<ReleasePeriodDto>> SetFinalSummaryRunAsync(string? periodName, short? finalSummariesRun, string? sendEmail);
    }
}