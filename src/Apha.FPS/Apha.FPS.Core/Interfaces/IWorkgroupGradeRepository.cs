using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    /// <summary>
    /// Repository interface for WorkgroupGrade CRUD and lookup operations.
    /// </summary>
    public interface IWorkgroupGradeRepository
    {
        /// <summary>Returns a paginated list of WorkgroupGrade records.</summary>
        Task<PagedData<WorkgroupGrade>> GetAllWorkgroupGradesPagedAsync(PaginationParameters<string> query, CancellationToken cancellationToken = default);

        /// <summary>Returns a single WorkgroupGrade by WgGrade code.</summary>
        Task<WorkgroupGrade?> GetByWgGradeAsync(string wgGrade, CancellationToken cancellationToken = default);

        /// <summary>Creates a new WorkgroupGrade record.</summary>
        Task<WorkgroupGrade> CreateAsync(WorkgroupGrade entity, CancellationToken cancellationToken = default);

        /// <summary>Updates an existing WorkgroupGrade record.</summary>
        Task<WorkgroupGrade> UpdateAsync(WorkgroupGrade entity, CancellationToken cancellationToken = default);

        /// <summary>Deletes a WorkgroupGrade record by WgGrade code.</summary>
        Task<bool> DeleteAsync(string wgGrade, CancellationToken cancellationToken = default);

        /// <summary>Returns all Profit Centre Grade codes for dropdown population.</summary>
        Task<List<string>> GetAllPcGradesAsync(CancellationToken cancellationToken = default);

        /// <summary>Returns all Grade codes for dropdown population.</summary>
        Task<List<string>> GetAllGradeCodesAsync(CancellationToken cancellationToken = default);

        /// <summary>Returns all Workgroup names for dropdown population.</summary>
        Task<List<string>> GetAllWorkgroupNamesAsync(CancellationToken cancellationToken = default);
    }
}
