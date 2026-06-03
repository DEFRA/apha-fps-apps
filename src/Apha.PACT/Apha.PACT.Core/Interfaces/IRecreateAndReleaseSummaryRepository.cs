using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Pagination;

namespace Apha.PACT.Core.Interfaces
{
    public interface IRecreateAndReleaseSummaryRepository
    {
        Task<PagedData<RecreateSummaryLogs>> GetRecreateSummariesLogsAsync(PaginationParameters<string> parameters);
    }
}
