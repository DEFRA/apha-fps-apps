using Apha.PACT.Core.Entities;

namespace Apha.PACT.Core.Interfaces
{
    public interface IProjectProfileRepository
    {
        Task<IList<ProjectProfile>> GetProfileDataAsync(string project);
        Task<IList<ProjectProfile>> GetCumulativeDataAsync(string project);
    }
}