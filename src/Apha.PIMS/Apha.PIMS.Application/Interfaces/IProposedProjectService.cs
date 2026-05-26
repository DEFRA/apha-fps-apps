using Apha.PIMS.Application.Dtos;

namespace Apha.PIMS.Application.Interfaces
{
    public interface IProposedProjectService
    {
        Task<ProjectDto?> GetFpsProjectByIdAsync(string parentproject);
        Task<ProposedProjectDto?> GetProposedProjectByIdAsync(string parentproject);
        Task<ProposedProjectDto> AddProposedProjectAsync(ProposedProjectDto dto);
        Task<List<string>> GetProjectProgramsAsync();
        Task<List<string>> GetProjectCustomersAsync();
        Task<List<string>> GetProjectStatusesAsync();
    }
}
