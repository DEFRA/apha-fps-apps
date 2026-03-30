using Apha.PACT.Application.Dtos;

namespace Apha.PACT.Application.Interfaces
{
    public interface IWorkGroupService
    {
        Task<IEnumerable<WorkGroupDto>> GetAllWorkGroupsAsync();
    }
}
