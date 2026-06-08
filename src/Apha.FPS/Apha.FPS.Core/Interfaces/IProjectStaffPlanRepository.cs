using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IProjectStaffPlanRepository
    {
        Task<PagedData<ProjectStaffPlanView>> GetPagedAsync(PaginationParameters<string> query);
    }
}
