using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IYearMasterRepository
    {
        Task<IEnumerable<YearMaster>> GetAllFpsYearsAsync();
        Task<PagedData<YearMaster>> GetAllFpsYearsPagedAsync(PaginationParameters<int> query);
        Task<YearMaster?> GetFpsYearByIdAsync(int fpsYear);

        /// <summary>
        /// Resolves the single active FPS year currently in "Open" status - the authoritative
        /// current year, independent of any ambient per-request context (e.g. the X-FPS-Year
        /// header, which reflects whichever year a UI session happens to be browsing, not
        /// necessarily the real current year).
        /// </summary>
        Task<YearMaster?> GetOpenFpsYearAsync();
    }
}
