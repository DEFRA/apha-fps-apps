using Apha.FPS.Application.Dtos;

namespace Apha.FPS.Application.Interfaces
{
    public interface IWorkGroupService
    {
        Task<List<WorkGroupViewDto>> GetWorkGroupsAsync(string profitCentre);
    }
}
