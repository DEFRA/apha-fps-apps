using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apha.FPSApps.Application.Interfaces.CostBookApiClients
{
    public interface ICostBookProjectApiClient
    {
        Task<ApiResponseDto<List<ProjectDto>>> GetFilteredProjectsAsync(QueryParameters<string> criteria);
        
        Task<ApiResponseDto<ProjectDto>> GetProjectByIdAsync(string id);
        Task<ApiResponseDto<ProjectDto>> AddProjectAsync(ProjectDto project);
        Task<ApiResponseDto<ProjectDto>> UpdateProjectAsync(string id, ProjectDto project);        
        Task<ApiResponseDto<bool>> DeleteProjectAsync(string id);
        Task<ApiResponseDto<ProjectDto>> CopyProjectAsync(string id, string newId);
        Task<ApiResponseDto<bool>> RecostProjectAsync(string id);
        Task<ApiResponseDto<string>> GetNextProjectNumberAsync(string? baseNumber);
    }
}
