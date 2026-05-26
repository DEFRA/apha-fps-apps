using Apha.PIMS.Core.Entities;

namespace Apha.PIMS.Core.Interfaces
{
    public interface IProposedProjectRepository
    {
        Task<Project?> GetFpsProjectByIdAsync(string parentproject);
        Task<ProposedProject?> GetProposedProjectByIdAsync(string parentproject);
        Task<ProposedProject> AddProposedProjectAsync(ProposedProject entity);
        Task<List<string>> GetProjectProgramsAsync();
        Task<List<string>> GetProjectCustomersAsync();
        Task<List<ProjectStatus>> GetProjectStatusesAsync();
    }
}
