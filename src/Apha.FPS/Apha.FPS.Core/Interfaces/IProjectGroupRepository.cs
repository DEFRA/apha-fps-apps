using Apha.FPS.Core.Enities;
using Apha.FPS.Core.Entities;

namespace Apha.FPS.Core.Interfaces
{
    public interface IProjectGroupRepository
    {
        Task<IEnumerable<ProjectGroup>> GetAllProjectGroupsAsync();
        Task<IEnumerable<ProjectGroup>> GetAllProjectGroupsByUserAsync();
    }
}
