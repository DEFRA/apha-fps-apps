using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;

namespace Apha.FPSApps.Application.Interfaces.PIMS
{
    public interface IProposedProjectService
    {
        Task<ApiResponseDto<ProposedProjectDto>> CreateProposedProjectAsync(ProposedProjectDto dto);
        Task<ApiResponseDto<List<string>>> GetProjectProgramsAsync();
        Task<ApiResponseDto<List<string>>> GetProjectCustomersAsync();
        Task<ApiResponseDto<List<string>>> GetProjectStatusesAsync();
    }
}
