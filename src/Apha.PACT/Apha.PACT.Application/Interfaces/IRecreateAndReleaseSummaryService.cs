using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;

namespace Apha.PACT.Application.Interfaces
{
    public interface IRecreateAndReleaseSummaryService
    {
        Task<PaginatedResult<RecreateSummariesLogDto>> GetRecreateSummariesAllLogsAsync(QueryParameters<string> query);
        Task<IReadOnlyList<ReleasePeriodDto>> GetReleaseSummariesAsync();
        Task<ReleasePeriodDto?> SetFinalSummaryRunAsync(string periodName, short finalSummariesRun);
    }
}
