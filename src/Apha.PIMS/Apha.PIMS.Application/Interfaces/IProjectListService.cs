using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Pagination;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.Application.Interfaces
{
    public interface IProjectListService
    {
        Task<PaginatedResult<ProjectListViewDto>> GetAllProjectsAsync(QueryParameters<string> query, int showWhichProjects = 2);
        Task<List<ProjectListViewDto>> GetAllProjectsForDropDownAsync();
        Task<ProjectDto?> GetFpsProjectByIdAsync(string parentproject);
        Task<ProposedProjectDto?> GetProposedProjectByIdAsync(string parentproject);
        Task<List<ProjectsDto>> GetYearlyDetailsByProjectAsync(string parentproject);
        Task<ProposedProjectDto> AddProjectAsync(ProposedProjectDto dto);
    }
}
