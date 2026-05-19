using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Pagination;

namespace Apha.PACT.Core.Interfaces
{
    public interface IWorkGroupRepository
    {
        Task<IEnumerable<WorkGroup>> GetAllWorkGroupsAsync();
        Task<PagedData<WorkGroupTimeCode>> GetWorkGroupTimeCodeAsync(PaginationParameters<string> query, string? workGroup, int? monthNumber);
    }
}
