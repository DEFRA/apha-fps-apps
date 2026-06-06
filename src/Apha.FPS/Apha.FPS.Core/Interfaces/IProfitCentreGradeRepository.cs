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

        /// <summary>
        /// Returns a paginated list of all profit centre grades for the current FPS year,
        /// supporting column filter and sort for the maintenance DataGrid.
        /// </summary>
        Task<PagedData<ProfitCentreGrade>> GetAllPagedAsync(PaginationParameters<string> query);

        /// <summary>Returns a single profit centre grade by PcGrade code (within the current FPS year).</summary>
        Task<ProfitCentreGrade?> GetByIdAsync(string pcGrade);

        /// <summary>Inserts a new profit centre grade row.</summary>
        Task<ProfitCentreGrade> CreateAsync(ProfitCentreGrade entity);

        /// <summary>Updates an existing profit centre grade row.</summary>
        Task<ProfitCentreGrade> UpdateAsync(string originalPcGrade, ProfitCentreGrade entity);

        /// <summary>Deletes a profit centre grade row by PcGrade code.</summary>
        Task<bool> DeleteAsync(string pcGrade);

        /// <summary>
        /// Checks whether the given ProfitCentre value exists in tblkpprofitcentre.
        /// Used to enforce the INSERT/UPDATE trigger constraint in .NET.
        /// </summary>
        Task<bool> ProfitCentreExistsAsync(string profitCentre);

        /// <summary>Returns all ProfitCentre codes from tblkpprofitcentre for the current FPS year.</summary>
        Task<List<string>> GetAllProfitCentreCodesAsync();

        /// <summary>Returns all Profit Centre Grade codes for dropdown population.</summary>
        Task<List<string>> GetAllPcGradesAsync();
    }
}
