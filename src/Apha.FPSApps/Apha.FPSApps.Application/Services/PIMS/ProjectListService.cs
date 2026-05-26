using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.PIMS
{
    public class ProjectListService : IProjectListService
    {
        private readonly IPimsApiClient _client;

        public ProjectListService(IPimsApiClient client)
        {
            _client = client;
        }

        public async Task<ApiResponseDto<List<ProjectListViewDto>>> GetAllProjectsAsync(QueryParameters<string> query, int filterOption = 2)
           => await _client.PimsProjectList.GetAllProjectsAsync(query, filterOption);

        public async Task<ApiResponseDto<List<ProjectListViewDto>>> GetAllProjectsListAsync()
            => await _client.PimsProjectList.GetAllProjectsListAsync();

        public async Task<ApiResponseDto<ProjectDto>> GetFpsProjectByIdAsync(string parentproject)
            => await _client.PimsProjectList.GetFpsProjectByIdAsync(parentproject);

        public async Task<ApiResponseDto<ProposedProjectDto>> GetProposedProjectByIdAsync(string parentproject)
            => await _client.PimsProjectList.GetProposedProjectByIdAsync(parentproject);

        public async Task<ApiResponseDto<List<ProjectsDto>>> GetYearlyDetailsByProjectAsync(string parentproject)
            => await _client.PimsProjectList.GetYearlyDetailsByProjectAsync(parentproject);
    }
}
