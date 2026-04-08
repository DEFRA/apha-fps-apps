using Apha.Costbook.Application.Pagination;
using Apha.Costbook.Application.Dtos;

namespace Apha.Costbook.Application.Interfaces
{
    public interface IProjectService
    {
       
        Task<PaginatedResult<ProjectDto>> GetPaginatedProjectsAsync(QueryParameters<string> queryFilter);
        
        Task<ProjectDto> GetProjectByIdAsync(string id);
        Task<ProjectDto> AddProjectAsync(ProjectDto dto);
        Task<ProjectDto> UpdateProjectAsync(string id, ProjectDto dto);
        Task<bool> DeleteProjectAsync(string id);
        Task<ProjectDto> CopyProjectAsync(string oldId, string newId);
        Task<bool> RecostProjectAsync(string id);
        Task<string> GetNextProjectNumberAsync(string? baseNumber);
    }
}
