using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    public interface IPimsProjectListApiClient
    {
        Task<ApiResponseDto<List<ProjectListViewDto>>> GetAllProjectsAsync(QueryParameters<string> query, int filterOption = 2);
        Task<ApiResponseDto<List<ProjectListViewDto>>> GetAllProjectsListAsync();
        Task<ApiResponseDto<ProjectDto>> GetFpsProjectByIdAsync(string parentproject);
        Task<ApiResponseDto<ProposedProjectDto>> GetProposedProjectByIdAsync(string parentproject);
        Task<ApiResponseDto<List<ProjectsDto>>> GetYearlyDetailsByProjectAsync(string parentproject);
        Task<ApiResponseDto<List<ProjectListMilestoneDto>>> GetAllProjectsForMilestoneAsync();
    }
}
