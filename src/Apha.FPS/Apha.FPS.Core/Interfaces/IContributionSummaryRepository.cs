using Apha.FPS.Core.Entities;

namespace Apha.FPS.Core.Interfaces
{
    public interface IContributionSummaryRepository
    {
        /// <summary>
        /// Returns all time-seller rows for the given selling profit centre,
        /// scoped to the current FPS year via the global query filter.
        /// Sorting (including the derived % Planned columns, where a zero Avail Hrs
        /// value is treated as 0) is applied here.
        /// </summary>
        Task<List<ContributionSummaryView>> GetBySellingPcAsync(string sellingPc, string? sortBy = null, bool descending = false);
    }
}
