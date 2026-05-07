using Apha.PACT.Core.Entities;

namespace Apha.PACT.Core.Interfaces
{
    public interface IProjectMonthRepository
    {
        Task<IList<Month>> GetMonthsAsync();
        Task<IList<ProjectMonth>> GetProjectMonthByProjectAsync(string project);
        Task<ProjectMonth?> GetProjectMonthAsync(string project, int monthNo);
        Task<ProjectMonth> CreateProjectMonthAsync(ProjectMonth entity);
        Task<ProjectMonth> UpdateProjectMonthAsync(ProjectMonth entity);
        Task<bool> DeleteProjectMonthAsync(string project, int monthNo);
    }
}