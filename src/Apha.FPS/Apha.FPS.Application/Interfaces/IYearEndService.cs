using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface IYearEndService
    {
        Task<PaginatedResult<BatchJobHistoryDto>> GetBatchJobsHistoryAsync(QueryParameters<string> query, string jobName);

        Task<bool> CanRunBatchJobAsync(string jobName);

        Task<BatchJobEventTriggerDto> TriggerYearEndInitiationJobAsync(int contextyear, string requestedBy, string correlationId);
    }
}
