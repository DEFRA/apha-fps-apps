using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    /// <summary>
    /// Repository interface for WorkgroupGrade CRUD and lookup operations.
    /// </summary>
    public interface IWorkGroupGradeRepository
    {
        /// <summary>Returns a paginated list of WorkgroupGrade records.</summary>
        Task<PagedData<WorkgroupGrade>> GetAllWorkgroupGradesPagedAsync(PaginationParameters<string> query);

        /// <summary>Returns a single WorkgroupGrade by WgGrade code.</summary>
        Task<WorkgroupGrade?> GetByWgGradeAsync(string wgGrade);

        /// <summary>Creates a new WorkgroupGrade record.</summary>
        Task<WorkgroupGrade> CreateAsync(WorkgroupGrade entity);

        /// <summary>Updates an existing WorkgroupGrade record.</summary>
        Task<WorkgroupGrade> UpdateAsync(WorkgroupGrade entity);

        /// <summary>Deletes a WorkgroupGrade record by WgGrade code.</summary>
        Task<bool> DeleteAsync(string wgGrade);

        /// <summary>Returns all Grade codes for dropdown population.</summary>
        Task<List<string>> GetAllGradeCodesAsync();

        // Existing methods for backward compatibility
        Task<PagedData<WorkGroupGradeView>> GetWorkGroupGradesAsync(PaginationParameters<string> query, string profitCentreGrade);
        Task<bool> DeleteWorkGroupGradeAsync(string wgGrade);
    }
}
