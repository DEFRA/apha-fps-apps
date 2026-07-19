using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IYearEndRepository
    {
        Task<PagedData<BatchJobHistory>> GetBatchJobsHistoryAsync(PaginationParameters<string> query, string jobName);

        Task<bool> CanRunBatchJobAsync(string jobName);

        Task<BatchJobQueue> EnqueueBatchJobAsync(string jobName, string requestedBy, string correlationId, string note);
    }
}
