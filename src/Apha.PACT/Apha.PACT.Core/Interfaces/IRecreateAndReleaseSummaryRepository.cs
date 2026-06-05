using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Pagination;

namespace Apha.PACT.Core.Interfaces
{
    public interface IRecreateAndReleaseSummaryRepository
    {
        Task<PagedData<RecreateSummaryLogWithComment>> GetRecreateSummaryLogAsync(PaginationParameters<string> parameters);
        Task<ReleaseSummary> GetReleaseSummariesAsync();
        Task<ReleasePeriod?> SetFinalSummaryRunAsync(string? periodName, short? finalSummariesRun, string? sendEmail);
    }
}
