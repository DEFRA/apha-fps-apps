using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IYearEndRepository
    {
        Task<PagedData<BatchJobHistory>> GetBatchJobsHistoryAsync(PaginationParameters<string> query, string jobName);

        Task<bool> CanInitiateYearEndDataSetupRequestAsync(string jobName);
        Task<bool> CanApproveYearEndDataSetupRequestAsync(string jobName);

        Task<BatchJobQueue> EnqueueDataSetupBatchJobAsync(string jobName, string requestedBy, string correlationId, string note);
    }
}
