using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;

namespace Apha.FPSApps.Application.Services.PACT
{
    public class ProjectProfileService : IProjectProfileService
    {
        private readonly IPactApiClient _pactClient;

        public ProjectProfileService(IPactApiClient pactClient)
        {
            _pactClient = pactClient;
        }

        public async Task<ApiResponseDto<List<ProjectProfileGraphDto>>> GetProfileGraphDataAsync(string project)
            => await _pactClient.PactProjectProfile.GetProfileGraphDataAsync(project);

        public async Task<ApiResponseDto<List<ProjectProfileCumulativeGraphDto>>> GetCumulativeGraphDataAsync(string project)
            => await _pactClient.PactProjectProfile.GetCumulativeGraphDataAsync(project);
    }
}
