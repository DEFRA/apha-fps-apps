using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    /// <summary>
    /// Frontend service interface for the Grade maintenance resource.
    /// Mirrors the five async methods on <see cref="Apha.FPSApps.Application.Interfaces.FpsApiClients.IFpsGradeApiClient"/>.
    /// Injected into <c>GradeMaintenanceController</c> in the FPS area.
    /// </summary>
    public interface IGradeService
    {
        // TRANSFORMENGINE: paginated list — delegates to IFpsGradeApiClient.GetAllPagedAsync
        /// <summary>
        /// Returns a paginated, optionally filtered and sorted list of grades for the active FPS year.
        /// </summary>
        Task<ApiResponseDto<List<GradeDto>>> GetAllPagedAsync(QueryParameters<string> query);

        // TRANSFORMENGINE: single record — delegates to IFpsGradeApiClient.GetByIdAsync
        /// <summary>
        /// Returns a single grade by its GradeCode for the active FPS year.
        /// </summary>
        Task<ApiResponseDto<GradeDto>> GetByIdAsync(string gradeCode);

        // TRANSFORMENGINE: create — delegates to IFpsGradeApiClient.CreateAsync
        /// <summary>
        /// Creates a new grade record.
        /// </summary>
        Task<ApiResponseDto<GradeDto>> CreateAsync(GradeDto dto);

        // TRANSFORMENGINE: update — originalCode in signature supports GradeCode rename
        /// <summary>
        /// Updates an existing grade record identified by <paramref name="originalCode"/>.
        /// The <paramref name="dto"/> may carry a new GradeCode value to trigger a rename.
        /// </summary>
        Task<ApiResponseDto<GradeDto>> UpdateAsync(string originalCode, GradeDto dto);

        // TRANSFORMENGINE: delete — delegates to IFpsGradeApiClient.DeleteAsync
        /// <summary>
        /// Deletes the grade with the given GradeCode in the active FPS year.
        /// </summary>
        Task<ApiResponseDto<bool>> DeleteAsync(string gradeCode);
    }
}
