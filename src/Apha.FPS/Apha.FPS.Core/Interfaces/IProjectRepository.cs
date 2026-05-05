using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IProjectRepository
    {
        Task<IEnumerable<ProjectView>> GetAllProjectsAsync();
        Task<IEnumerable<PactProjectView>> GetAllPactProjectsAsync();
        Task<PagedData<Project>> GetPagedProjectsAsync(PaginationParameters<string> query);
        Task<PagedData<PactProjectView>> GetPagedPactProjectsAsync(PaginationParameters<string> query);
        Task<Project?> GetProjectByIdAsync(string parentProject);
        Task<Project> CreateProjectAsync(Project project);
        Task<Project> UpdateProjectAsync(Project project);
        Task<Project?> UpdatePactProjectDetailsAsync(Project project);
        Task<Project?> UpdatePactPortfolioDetailsAsync(Project project);
        Task<bool> DeleteProjectAsync(string parentProject);
        Task<bool> HasAssociatedJobCodesAsync(string parentProject);
        Task<PagedData<Project>> GetProjectsByProgramAsync(PaginationParameters<string> query, string programNo);
    }
}
