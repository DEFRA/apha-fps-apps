using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using Apha.FPSApps.Application.Interfaces.Costbook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.Costbook
{
    public class CostBookProjectService : ICostBookProjectService
    {
        private readonly ICostBookApiClient _costBookClient;

        public CostBookProjectService(ICostBookApiClient costBookClient)
        {
            _costBookClient = costBookClient;
        }
        public async Task<ApiResponseDto<List<ProjectDto>>> GetFilteredProjectsAsync(QueryParameters<string> criteria)
        {
            var response = await _costBookClient.Projects.GetFilteredProjectsAsync(criteria);
            return response;
        }
        public Task<ApiResponseDto<ProjectDto>> GetProjectByIdAsync(string id)
        {
            var response = _costBookClient.Projects.GetProjectByIdAsync(id);
            return response;
        }

        public Task<ApiResponseDto<ProjectDto>> AddProjectAsync(ProjectDto project)
        {
            // Apply business logic: Set audit fields
            project.CreatedDate = DateTime.UtcNow;
            project.Status = "Active";

            var response = _costBookClient.Projects.AddProjectAsync(project);
            return response;
        }

        public Task<ApiResponseDto<ProjectDto>> UpdateProjectAsync(string id, ProjectDto project)
        {
            // Apply business logic: Set audit fields
            project.ModifiedDate = DateTime.UtcNow;

            var response = _costBookClient.Projects.UpdateProjectAsync(id, project);
            return response;
        }

        public Task<ApiResponseDto<bool>> DeleteProjectAsync(string id)
        {
            var response = _costBookClient.Projects.DeleteProjectAsync(id);
            return response;
        }

        public Task<ApiResponseDto<ProjectDto>> CopyProjectAsync(string id, string newId)
        {
            var response = _costBookClient.Projects.CopyProjectAsync(id, newId);
            return response;
        }

        public Task<ApiResponseDto<bool>> RecostProjectAsync(string id)
        {
            var response = _costBookClient.Projects.RecostProjectAsync(id);
            return response;
        }

        public Task<ApiResponseDto<string>> GetNextProjectNumberAsync(string? baseNumber)
        {
            var response = _costBookClient.Projects.GetNextProjectNumberAsync(baseNumber);
            return response;
        }
    }
}
