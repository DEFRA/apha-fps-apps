using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Pagination;

namespace Apha.PIMS.Core.Interfaces
{
    public interface IProjectListRepository
    {
        Task<PagedData<ProjectListView>> GetAllProjectsAsync(PaginationParameters<string> queryFilter, int showWhichProjects);
        Task<Project?> GetFpsProjectByIdAsync(string parentproject);
        Task<ProposedProject?> GetProposedProjectByIdAsync(string parentproject);
        Task<List<Projects>> GetYearlyDetailsByProjectAsync(string parentproject);
        Task<ProposedProject> AddProjectAsync(ProposedProject entity);
        Task<List<ProjectListView>> GetAllProjectsForDropDownAsync();
    }
}
