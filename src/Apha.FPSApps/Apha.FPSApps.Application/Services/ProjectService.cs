using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IFpsApiClient _fpsApiClient;

        public ProjectService(IFpsApiClient fpsApiClient)
        {
            _fpsApiClient = fpsApiClient;
        }

        public async Task<ApiResponseDto<List<ProjectDto>>> GetProjectsByProgramAsync(QueryParameters<string> query, string programNo)
        {
            return await _fpsApiClient.FpsProject.GetProjectsByProgramAsync(query, programNo);
        }
    }
}
