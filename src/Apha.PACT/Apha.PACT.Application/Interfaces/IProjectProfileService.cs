using Apha.PACT.Application.Dtos;

namespace Apha.PACT.Application.Interfaces
{
    public interface IProjectProfileService
    {
        Task<IList<ProjectProfileDto>> GetProfileDataAsync(string project);
        Task<IList<ProjectProfileCumulativeDto>> GetCumulativeDataAsync(string project);
    }
}