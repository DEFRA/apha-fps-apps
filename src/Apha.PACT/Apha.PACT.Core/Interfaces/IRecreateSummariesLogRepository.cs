using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Pagination;

namespace Apha.PACT.Core.Interfaces
{
    public interface IRecreateSummariesLogRepository
    {
        Task<PagedData<RecreateSummariesLog>> GetRecreateSummariesAllLogsAsync(PaginationParameters<string> parameters);
        Task<IReadOnlyList<ReleasePeriod>> GetReleaseSummariesAsync();
        Task<ReleasePeriod?> SetFinalSummaryRunAsync(string periodName, short finalSummariesRun);
    }
}
