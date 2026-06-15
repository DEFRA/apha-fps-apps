using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    /// <summary>
    /// API client interface for the Grade maintenance resource.
    /// Mirrors the five REST endpoints on backend GradeController (route: api/v1/Grade).
    /// Injected into frontend services via IFpsApiClient.FpsGrade.
    /// </summary>
    public interface IFpsGradeApiClient
    {
        // TRANSFORMENGINE: GET api/v1/Grade/paged — paginated list; maps to GradeController.GetAllPagedAsync
        /// <summary>
        /// Returns a paginated, optionally filtered and sorted list of grades for the active FPS year.
        /// Calls GET api/v1/Grade/paged.
        /// </summary>
        Task<ApiResponseDto<List<GradeDto>>> GetAllPagedAsync(QueryParameters<string> query);

        // TRANSFORMENGINE: GET api/v1/Grade/{gradeCode} — single record by PK; maps to GradeController.GetByIdAsync
        /// <summary>
        /// Returns a single grade by its GradeCode for the active FPS year.
        /// Calls GET api/v1/Grade/{gradeCode}.
        /// </summary>
        Task<ApiResponseDto<GradeDto>> GetByIdAsync(string gradeCode);

        // TRANSFORMENGINE: POST api/v1/Grade — create new record; maps to GradeController.CreateAsync
        /// <summary>
        /// Creates a new grade record.
        /// Calls POST api/v1/Grade.
        /// </summary>
        Task<ApiResponseDto<GradeDto>> CreateAsync(GradeDto dto);

        // TRANSFORMENGINE: PUT api/v1/Grade/{gradeCode} — update; originalCode in path supports GradeCode rename
        /// <summary>
        /// Updates an existing grade record identified by <paramref name="originalCode"/>.
        /// The <paramref name="dto"/> may carry a new GradeCode value to trigger a rename.
        /// Calls PUT api/v1/Grade/{originalCode}.
        /// </summary>
        Task<ApiResponseDto<GradeDto>> UpdateAsync(string originalCode, GradeDto dto);

        // TRANSFORMENGINE: DELETE api/v1/Grade/{gradeCode} — delete by PK; maps to GradeController.DeleteAsync
        /// <summary>
        /// Deletes the grade with the given GradeCode in the active FPS year.
        /// Calls DELETE api/v1/Grade/{gradeCode}.
        /// </summary>
        Task<ApiResponseDto<bool>> DeleteAsync(string gradeCode);
    }
}
