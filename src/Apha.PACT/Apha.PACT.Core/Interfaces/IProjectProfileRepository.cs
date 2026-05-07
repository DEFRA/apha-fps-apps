using Apha.PACT.Core.Entities;

namespace Apha.PACT.Core.Interfaces
{
    public interface IProjectProfileRepository
    {
        Task<IList<ProjectProfile>> GetProfileGraphDataAsync(string project);
        Task<IList<ProjectProfile>> GetCumulativeGraphDataAsync(string project);
    }
}