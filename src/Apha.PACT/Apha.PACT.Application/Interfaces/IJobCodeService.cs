using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;

namespace Apha.PACT.Application.Interfaces
{
    public interface IJobCodeService
    {
        Task<IEnumerable<JobCodeDto>> GetJobCodesByProjectAsync(string parentProject);
        Task<PaginatedResult<JobCodeDto>> GetPagedJobCodesAsync(QueryParameters<string> query, string? parentProject);
        Task<JobCodeDto?> GetJobCodeByIdAsync(string jobCodeId);
        Task<JobCodeDto> CreateJobCodeAsync(JobCodeDto jobCode);
        Task<JobCodeDto> UpdateJobCodeAsync(JobCodeDto jobCode);
        Task<bool> DeleteJobCodeAsync(string jobCodeId);
    }
}
