using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Entities;

namespace Apha.FPS.Application.Interfaces
{
    public interface IProjectService
    {
        Task<IEnumerable<ProjectDto>> GetAllProjectsAsync();
        Task<PaginatedResult<ProjectDto>> GetPagedProjectsAsync(QueryParameters<string> query);
        Task<PaginatedResult<ProjectDto>> GetPagedPactProjectsAsync(QueryParameters<string> query);
        Task<ProjectDto?> GetProjectByIdAsync(string parentProject);
        Task<ProjectDto> CreateProjectAsync(ProjectDto projectDto);
        Task<ProjectDto> UpdateProjectAsync(ProjectDto projectDto);
        Task<ProjectDto?> UpdatePactProjectDetailsAsync(ProjectDto projectDto);
        Task<bool> DeleteProjectAsync(string parentProject);        
    }
}
