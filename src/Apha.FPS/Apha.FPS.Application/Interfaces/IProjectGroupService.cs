using Apha.FPS.Application.Dtos;

namespace Apha.FPS.Application.Interfaces
{
    public interface IProjectGroupService
    {
        Task<IEnumerable<ProjectGroupDto>> GetAllProjectGroupsAsync();
    }
}
