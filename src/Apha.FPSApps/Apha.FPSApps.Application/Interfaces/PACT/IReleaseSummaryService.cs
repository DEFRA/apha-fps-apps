using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;

namespace Apha.FPSApps.Application.Interfaces.PACT
{
    public interface IReleaseSummaryService
    {
        Task<ApiResponseDto<ReleaseSummaryDto>> GetReleaseSummariesAsync();
        Task<ApiResponseDto<ReleasePeriodDto>> SetFinalSummaryRunAsync(string? periodName, short? finalSummariesRun, string? sendEmail);
    }
}