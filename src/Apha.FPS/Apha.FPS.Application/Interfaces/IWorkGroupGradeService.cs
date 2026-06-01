using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    /// <summary>
    /// Service interface for WorkgroupGrade CRUD and lookup operations.
    /// </summary>
    public interface IWorkGroupGradeService
    {
        /// <summary>Returns a paginated list of WorkgroupGrade records.</summary>
        Task<PaginatedResult<WorkgroupGradeDto>> GetAllWorkgroupGradesPagedAsync(QueryParameters<string> query, CancellationToken cancellationToken = default);

        /// <summary>Returns a single WorkgroupGrade by WgGrade code.</summary>
        Task<WorkgroupGradeDto?> GetByWgGradeAsync(string wgGrade, CancellationToken cancellationToken = default);

        /// <summary>Creates a new WorkgroupGrade record.</summary>
        Task<WorkgroupGradeDto> CreateAsync(WorkgroupGradeDto dto, CancellationToken cancellationToken = default);

        /// <summary>Updates an existing WorkgroupGrade record.</summary>
        Task<WorkgroupGradeDto> UpdateAsync(WorkgroupGradeDto dto, CancellationToken cancellationToken = default);

        /// <summary>Deletes a WorkgroupGrade record by WgGrade code.</summary>
        Task<bool> DeleteAsync(string wgGrade, CancellationToken cancellationToken = default);

        /// <summary>Returns all Profit Centre Grade codes for dropdown population.</summary>
        Task<List<string>> GetAllPcGradesAsync(CancellationToken cancellationToken = default);

        /// <summary>Returns all Grade codes for dropdown population.</summary>
        Task<List<string>> GetAllGradeCodesAsync(CancellationToken cancellationToken = default);

        /// <summary>Returns all Workgroup names for dropdown population.</summary>
        Task<List<string>> GetAllWorkgroupNamesAsync(CancellationToken cancellationToken = default);

        // Existing methods for backward compatibility
        Task<PaginatedResult<WorkgroupGradeDto>> GetWorkGroupGradeAsync(QueryParameters<string> query, string profitCentreGrade);
        Task<bool> DeleteWorkGroupGradeAsync(string wgGrade);
    }
}
