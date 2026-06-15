using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface IProjectGroupStaffPlanService
    {
        Task<PaginatedResult<ProjectGroupStaffPlanViewDto>> GetPagedAsync(QueryParameters<string> query);
    }
}
