using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Pagination;

namespace Apha.PIMS.Core.Interfaces
{
    public interface IProjectListRepository
    {
        Task<PagedData<ProjectListView>> GetAllProjectsAsync(PaginationParameters<string> queryFilter, int showWhichProjects);
        Task<List<Projects>> GetYearlyDetailsByProjectAsync(string parentproject);
        Task<List<ProjectListView>> GetAllProjectsForDropDownAsync();
        Task<List<ProjectListMilestone>> GetAllProjectsForMilestone();
        Task<ProjectDetailsMilestone?> GetProjectsDetailsForMilestoneAsync(string parentproject);
    }
}
