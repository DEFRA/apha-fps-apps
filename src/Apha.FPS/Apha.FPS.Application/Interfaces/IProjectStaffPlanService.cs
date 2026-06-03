using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface IProjectStaffPlanService
    {
        Task<PaginatedResult<ProjectStaffPlanViewDto>> GetPagedAsync(QueryParameters<string> query);
    }
}
