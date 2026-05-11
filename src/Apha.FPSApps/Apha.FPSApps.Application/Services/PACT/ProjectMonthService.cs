using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;

namespace Apha.FPSApps.Application.Services.PACT
{
    public class ProjectMonthService : IProjectMonthService
    {
        private readonly IPactApiClient _pactClient;

        public ProjectMonthService(IPactApiClient pactClient)
        {
            _pactClient = pactClient;
        }

        public async Task<ApiResponseDto<List<ProjectMonthDto>>> GetProjectMonthByProjectAsync(string project)
            => await _pactClient.PactProjectMonth.GetProjectMonthByProjectAsync(project);

        public async Task<ApiResponseDto<ProjectMonthDto>> GetProjectMonthAsync(string project, int monthNo)
            => await _pactClient.PactProjectMonth.GetProjectMonthAsync(project, monthNo);

        public async Task<ApiResponseDto<ProjectMonthDto>> CreateProjectMonthAsync(ProjectMonthDto dto)
            => await _pactClient.PactProjectMonth.CreateProjectMonthAsync(dto);

        public async Task<ApiResponseDto<ProjectMonthDto>> UpdateProjectMonthAsync(ProjectMonthDto dto)
            => await _pactClient.PactProjectMonth.UpdateProjectMonthAsync(dto);

        public async Task<ApiResponseDto<bool>> DeleteProjectMonthAsync(string project, int monthNo)
            => await _pactClient.PactProjectMonth.DeleteProjectMonthAsync(project, monthNo);
    }
}
