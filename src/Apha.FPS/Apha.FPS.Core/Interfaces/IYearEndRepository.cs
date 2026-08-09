using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IYearEndRepository
    {
        Task<PagedData<BatchJobHistory>> GetBatchJobsHistoryAsync(PaginationParameters<string> query, string jobName);

        Task<bool> CanInitiateYearEndDataSetupRequestAsync(string jobName);
       
        Task<bool> CanApproveOrRejectYearEndDataSetupRequestAsync(string jobName);
        
        Task<string> GetYearEndDataSetupRequestInitiatorAsync(string jobName);
        
        Task<BatchJobQueue> EnqueueDataSetupInitiationBatchJobAsync(string jobName, string requestedBy, string correlationId, string note);
        
        Task<BatchJobQueue> EnqueueDataSetupApprovalBatchJobAsync(string jobName, string requestedBy, string correlationId, string note);
        
        Task<BatchJobQueue> EnqueueDataSetupRejectBatchJobAsync(string jobName, string requestedBy, string correlationId, string note);

        Task<bool> CanInitiateYearEndCutOverRequestAsync(string jobName);

        Task<bool> CanApproveYearEndCutOverRequestAsync(string jobName);

        Task<string> GetYearEndCutOverRequestInitiatorAsync(string jobName);

        Task<BatchJobQueue> EnqueueCutOverInitiationBatchJobAsync(string jobName, string requestedBy, string correlationId, string note);

        Task<BatchJobQueue> EnqueueCutOverApprovalBatchJobAsync(string jobName, string requestedBy, string correlationId, string note);
    }
}
