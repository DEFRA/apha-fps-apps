// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — IGradeService.cs (new file)
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet8-mvc-e2e  Phase 8 — Frontend Service Interface + Implementation (Steps 12-13)
 * Migrated : 2026-06-10
 *
 * CHANGED:
 *   - New frontend service interface created for the Grade maintenance form (frmMaintGrade)
 *   - Five async CRUD method signatures mirror IFpsGradeApiClient exactly
 *   - All return types wrapped in ApiResponseDto<T> per FPSApps response envelope convention
 *   - UpdateAsync carries originalCode string to support GradeCode rename
 *     (matches PUT api/v1/Grade/{gradeCode} backend action signature)
 *   - No lookup method added: GradeController exposes no dedicated /grades lookup endpoint yet
 *     (deferred — see IFpsGradeApiClient TRANSFORMENGINE annotation)
 *
 * PRESERVED:
 *   - Signature parity with IFpsGradeApiClient (Phase 7 artefact)
 *   - QueryParameters<string> used for paginated list (consistent with all other FPS service interfaces)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: If a GET /Grade/grades lookup endpoint is added to GradeController,
 *     add Task<ApiResponseDto<List<string>>> GetAllGradeCodesAsync() here and in GradeService.cs.
 *   - TRANSFORMENGINE TODO: Register IGradeService → GradeService in
 *     Apha.FPSApps.Web/Extensions/ServiceCollectionExtension.cs AddServices() (PENDING in Interface changes log).
 */

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
