using Apha.FPS.Core.Entities;

namespace Apha.FPS.Core.Interfaces
{
    /// <summary>
    /// Repository interface for Agency operations.
    /// </summary>
    public interface IAgencyRepository
    {
        /// <summary>
        /// Gets all agencies.
        /// </summary>
        /// <returns>A list of all agencies.</returns>
        Task<IEnumerable<Agency>> GetAllAsync();

        /// <summary>
        /// Gets an agency by ID.
        /// </summary>
        /// <param name="agencyId">The agency identifier.</param>
        /// <returns>The agency if found, otherwise null.</returns>
        Task<Agency?> GetByIdAsync(int agencyId);
    }
}
