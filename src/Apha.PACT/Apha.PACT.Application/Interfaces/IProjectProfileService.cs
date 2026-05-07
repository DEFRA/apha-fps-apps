using Apha.PACT.Application.Dtos;

namespace Apha.PACT.Application.Interfaces
{
    public interface IProjectProfileService
    {
        Task<IList<ProjectProfileGraphDto>> GetProfileGraphDataAsync(string project);
        Task<IList<ProjectProfileCumulativeGraphDto>> GetCumulativeGraphDataAsync(string project);
    }
}