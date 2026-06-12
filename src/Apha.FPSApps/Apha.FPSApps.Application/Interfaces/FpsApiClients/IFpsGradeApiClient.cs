// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — IFpsGradeApiClient.cs (new file)
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet8-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-06-10
 *
 * CHANGED:
 *   - New frontend API client interface created for the Grade maintenance form (frmMaintGrade)
 *   - Five async methods mirroring the five REST endpoints on backend GradeController
 *     (route: api/v{version:apiVersion}/Grade)
 *   - All return types wrapped in ApiResponseDto<T> per FPSApps response envelope convention
 *   - QueryParameters<string> used for the paginated list endpoint (matches GradeController.GetAllPagedAsync signature)
 *   - UpdateAsync carries originalCode string to support GradeCode rename (matches PUT /{gradeCode} signature)
 *
 * PRESERVED:
 *   - Endpoint semantics match backend GradeController exactly:
 *       GET  api/v1/Grade/paged       → GetAllPagedAsync
 *       GET  api/v1/Grade/{gradeCode} → GetByIdAsync
 *       POST api/v1/Grade             → CreateAsync
 *       PUT  api/v1/Grade/{gradeCode} → UpdateAsync
 *       DELETE api/v1/Grade/{gradeCode} → DeleteAsync
 *   - No lookup-only endpoint added: GradeController has no dedicated /grades or /lookup endpoint yet
 *     (backend TODO deferred — see GradeController TRANSFORMENGINE annotation)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: If a GET /Grade/grades lookup endpoint is added to GradeController to serve
 *     grade-code dropdowns in DivisionGrade, WorkgroupGrade, and ProfitCentreGrade forms, add a
 *     corresponding Task<ApiResponseDto<List<string>>> GetAllGradeCodesAsync() method here.
 *   - TRANSFORMENGINE TODO: Register IFpsGradeApiClient → FpsGradeApiClient in
 *     Apha.FPSApps.Infrastructure ServiceCollectionExtension.AddHttpClients() before running.
 *   - TRANSFORMENGINE TODO: Add FpsGrade property to IFpsApiClient.cs and FpsApiClient.cs
 *     (Interface changes log entries for those files are PENDING).
 */

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
