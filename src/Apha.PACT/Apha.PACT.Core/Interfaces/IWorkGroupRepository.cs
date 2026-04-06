using Apha.PACT.Core.Entities;

namespace Apha.PACT.Core.Interfaces
{
    public interface IWorkGroupRepository
    {
        Task<IEnumerable<WorkGroup>> GetAllWorkGroupsAsync();
    }
}
