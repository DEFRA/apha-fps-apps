using Apha.FPS.Application.Dtos;

namespace Apha.FPS.Application.Interfaces
{
    /// <summary>
    /// Service interface for Agency operations.
    /// </summary>
    public interface IAgencyService
    {
        /// <summary>
        /// Gets all agencies.
        /// </summary>
        /// <returns>A list of all agencies.</returns>
        Task<IEnumerable<AgencyDto>> GetAllAgenciesAsync();
    }
}
