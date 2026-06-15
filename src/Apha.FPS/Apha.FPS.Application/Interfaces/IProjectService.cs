using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface IProjectService
    {
        Task<IEnumerable<ProjectDto>> GetAllProjectsAsync();
        Task<IEnumerable<ProjectDto>> GetAllPactProjectsAsync();
        Task<PaginatedResult<ProjectDto>> GetPagedProjectsAsync(QueryParameters<string> query);
        Task<PaginatedResult<ProjectDto>> GetPagedPactProjectsAsync(QueryParameters<string> query);
        Task<ProjectDto?> GetProjectByIdAsync(string parentProject);
        Task<ProjectDto> CreateProjectAsync(ProjectDto projectDto);
        Task<ProjectDto> UpdateProjectAsync(ProjectDto projectDto);
        Task<ProjectDto?> UpdatePactProjectDetailsAsync(ProjectDto projectDto);
        Task<ProjectDto?> UpdatePactPortfolioDetailsAsync(ProjectDto projectDto);
        Task<ProjectDto?> UpdateFpsPortfolioDetailsAsync(ProjectDto projectDto);
        Task<bool> DeleteProjectAsync(string parentProject);
        Task<PaginatedResult<ProjectDto>> GetProjectsByProgramAsync(QueryParameters<string> query, string programNo);
        Task<PaginatedResult<ProjectDto>> GetProjectsByProjectGroupAsync(QueryParameters<string> query, string projectGroup);

        // ProgrammeNewProject operations
        Task<bool> CheckProjectExistsAsync(string newProject);
        Task<bool> CheckProjectExistsInFarmFileAsync(string oldProject);
        Task ChangeProjectCodeAsync(string oldCode, string newCode);
        Task DeleteProjectAndChildrenAsync(string parentProject);

        Task<PaginatedResult<ProjectProfitabilityDto>> GetProjectProfitabilityAsync(QueryParameters<string> query, string programNo, string workTypeFilter);
        Task<PaginatedResult<ProjectProfitabilityDto>> GetProjectGroupProfitabilityAsync(QueryParameters<string> query, string projectGroup, string workTypeFilter);

        // TRANSFORMENGINE: new method — VLA-filtered project profitability list
        //   Added for frmJobcodeTotalsVLA migration. Filter dimensions: ProjectStatus,
        //   ProgramNo, Manager, Customer (from ProjectProfitabilityVlaReq carried
        //   inside QueryParameters<T>.Filter).
        Task<PaginatedResult<ProjectProfitabilityVlaDto>> GetProjectProfitabilityVlaAsync(QueryParameters<ProjectProfitabilityVlaReq> query);
    }
}
