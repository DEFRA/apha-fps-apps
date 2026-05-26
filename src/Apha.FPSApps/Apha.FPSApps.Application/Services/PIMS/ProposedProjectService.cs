using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;

namespace Apha.FPSApps.Application.Services.PIMS
{
    public class ProposedProjectService : IProposedProjectService
    {
        private readonly IPimsApiClient _client;

        public ProposedProjectService(IPimsApiClient client)
        {
            _client = client;
        }

        public async Task<ApiResponseDto<ProposedProjectDto>> CreateProposedProjectAsync(ProposedProjectDto dto)
            => await _client.PimsProposedProject.CreateProposedProjectAsync(dto);

        public async Task<ApiResponseDto<List<string>>> GetProjectProgramsAsync()
            => await _client.PimsProposedProject.GetProjectProgramsAsync();

        public async Task<ApiResponseDto<List<string>>> GetProjectCustomersAsync()
            => await _client.PimsProposedProject.GetProjectCustomersAsync();

        public async Task<ApiResponseDto<List<string>>> GetProjectStatusesAsync()
            => await _client.PimsProposedProject.GetProjectStatusesAsync();
    }
}
