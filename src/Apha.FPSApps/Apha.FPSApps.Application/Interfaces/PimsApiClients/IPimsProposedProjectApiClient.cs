using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    public interface IPimsProposedProjectApiClient
    {
        Task<ApiResponseDto<ProposedProjectDto>> CreateProposedProjectAsync(ProposedProjectDto dto);
        Task<ApiResponseDto<List<string>>> GetProjectProgramsAsync();
        Task<ApiResponseDto<List<string>>> GetProjectCustomersAsync();
        Task<ApiResponseDto<List<string>>> GetProjectStatusesAsync();
    }
}
