using Apha.FPS.Core.Entities;

namespace Apha.FPS.Core.Interfaces
{
    public interface IWorkGroupRepository
    {
        Task<List<WorkGroupView>> GetWorkGroupsAsync(string profitCentre);
    }
}
