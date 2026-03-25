using Apha.FPS.Core.Entities;

namespace Apha.FPS.Core.Interfaces
{
    public interface IProjectRepository
    {
        Task<IEnumerable<Project>> GetAllProjectsAsync();
        Task<Project?> GetProjectByIdAsync(string parentProject);
    }
}
