using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IProjectGroupStaffPlanRepository
    {
        Task<PagedData<ProjectGroupStaffPlanView>> GetPagedAsync(PaginationParameters<string> query);
    }
}
