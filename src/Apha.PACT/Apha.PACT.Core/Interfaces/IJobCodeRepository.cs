using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Pagination;

namespace Apha.PACT.Core.Interfaces
{
    public interface IJobCodeRepository
    {
        Task<IEnumerable<JobCode>> GetJobCodesByProjectAsync(string parentProject);
        Task<PagedData<JobCode>> GetPagedJobCodesAsync(PaginationParameters<string> query, string? parentProject);
        Task<JobCode?> GetJobCodeByIdAsync(string jobCodeId);
        Task<JobCode> CreateJobCodeAsync(JobCode jobCode);
        Task<JobCode> UpdateJobCodeAsync(JobCode jobCode);
        Task<bool> DeleteJobCodeAsync(string jobCodeId);
    }
}
