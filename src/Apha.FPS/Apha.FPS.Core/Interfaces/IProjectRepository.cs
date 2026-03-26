using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IProjectRepository
    {
        Task<IEnumerable<Project>> GetAllProjectsAsync();
        Task<Project?> GetProjectByIdAsync(string parentProject);
        Task<PagedData<Project>> GetProjectsByProgramAsync(PaginationParameters<string> query, string programNo);
    }
}
