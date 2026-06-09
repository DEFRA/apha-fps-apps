using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;

namespace Apha.PACT.Application.Interfaces
{
    public interface IRecreateAndReleaseSummaryService
    {
        Task<PaginatedResult<RecreateSummaryLogDto>> GetRecreateSummaryLogAsync(QueryParameters<string> query);
        Task<ReleaseSummaryDto> GetReleaseSummariesAsync();
        Task<IReadOnlyList<ReleasePeriodDto>> GetReleasePeriodsAsync();
        Task<ReleasePeriodDto?> SetFinalSummaryRunAsync(string? periodName, short? finalSummariesRun, string? sendEmail);
    }

}
