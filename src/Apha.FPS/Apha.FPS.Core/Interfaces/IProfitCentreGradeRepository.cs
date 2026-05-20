using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IProfitCentreGradeRepository
    {
        /// <summary>
        /// Returns a paginated list of profit centre grades filtered by the
        /// selected profit centre (from the dropdown) and the logged-in user's email,
        /// ordered by ChargeRate DESC.
        /// </summary>
        Task<PagedData<ProfitCentreGrade>> GetProfitCentreGradesAsync(
            PaginationParameters<string> query,
            string profitCentre);
    }
}
