using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    /// <summary>
    /// Repository interface for async CRUD and paged query operations on <see cref="Grade"/>.
    /// Implementations must respect the FpsYear query filter applied by FpsDbContext.
    /// </summary>
    public interface IGradeRepository
    {
        // TRANSFORMENGINE: GetAllPagedAsync — paged list with optional search/sort; string filter covers GradeCode + DescLong
        /// <summary>Returns a paged, optionally filtered and sorted list of grades for the active FPS year.</summary>
        Task<PagedData<Grade>> GetAllPagedAsync(PaginationParameters<string> query);

        // TRANSFORMENGINE: GetByIdAsync — look up single grade by GradeCode; FpsYear resolved via DbContext HasQueryFilter
        /// <summary>Returns a single grade by its GradeCode, or null if not found in the active FPS year.</summary>
        Task<Grade?> GetByIdAsync(string gradeCode);

        // TRANSFORMENGINE: CreateAsync — insert new grade record
        /// <summary>Inserts a new grade record and returns the persisted entity.</summary>
        Task<Grade> CreateAsync(Grade grade);

        // TRANSFORMENGINE: UpdateAsync — update existing grade; originalCode allows GradeCode rename if supported
        /// <summary>Updates an existing grade identified by <paramref name="originalCode"/> and returns the updated entity.</summary>
        Task<Grade> UpdateAsync(string originalCode, Grade grade);

        // TRANSFORMENGINE: DeleteAsync — remove grade by GradeCode; FpsYear resolved via DbContext HasQueryFilter
        /// <summary>Deletes the grade with the given GradeCode. Returns true if deleted, false if not found.</summary>
        Task<bool> DeleteAsync(string gradeCode);
    }
}
