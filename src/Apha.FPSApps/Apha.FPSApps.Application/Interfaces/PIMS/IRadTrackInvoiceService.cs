// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — IRadTrackInvoiceService.cs (new file)
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 8 — Frontend Service Interface + Implementation (Steps 12-13)
 * Migrated : 2026-06-12
 *
 * CHANGED:
 *   - New file: frontend service interface for RadTrack Invoice CRUD operations.
 *   - Signatures mirror IPimsRadTrackInvoiceApiClient exactly so the MVC controller
 *     depends only on IRadTrackInvoiceService (thin delegate pattern).
 *   - GetAllAsync includes four filter parameters (project, contract, year, program)
 *     matching the frmpimsinvoices.html toolbar dropdowns and backend query string.
 *   - GetTotalsAsync accepts the same four filter parameters as GetAllAsync so the
 *     footer totals row always matches the current grid filter.
 *   - UpdateAsync carries an explicit int id parameter matching the backend
 *     PUT api/v1/radtrackinvoice/{id} route requirement.
 *   - DeleteAsync returns ApiResponseDto<object> mirroring the backend anonymous
 *     { success: bool } response and the established IPimsMilestoneApiClient pattern.
 *
 * PRESERVED:
 *   - All return types wrapped in ApiResponseDto<T> per frontend convention.
 *   - Nullable filter parameters preserved (project, contract, year, program)
 *     so the controller can pass null when no filter is selected.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: If year filter type changes (int? vs string?) after
 *     Phase 9 HTTP client verification, update the year parameter type here and in
 *     RadTrackInvoiceService.cs consistently.
 *   - TRANSFORMENGINE TODO: Verify DeleteAsync return type (ApiResponseDto<object>)
 *     is consistent with frontend MVC controller expectations after Phase 9.
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PIMS
{
    public interface IRadTrackInvoiceService
    {
        // TRANSFORMENGINE: GET list — paged + filtered invoice grid.
        // Filter params mirror the four frmpimsinvoices.html toolbar dropdowns:
        // Project, Surveillance Contract, Year, Program.
        Task<ApiResponseDto<List<RadTrackInvoiceDto>>> GetAllAsync(
            QueryParameters<string> query,
            string? project = null,
            string? contract = null,
            int? year = null,
            string? program = null);

        // TRANSFORMENGINE: GET totals — aggregate footer row matching current filter.
        // Same four filter dimensions as GetAllAsync.
        Task<ApiResponseDto<RadTrackInvoiceTotalsDto>> GetTotalsAsync(
            string? project = null,
            string? contract = null,
            int? year = null,
            string? program = null);

        // TRANSFORMENGINE: GET by PK — used by Edit and Delete modal open flows.
        Task<ApiResponseDto<RadTrackInvoiceDto>> GetByIdAsync(int id);

        // TRANSFORMENGINE: POST create — Add Invoice modal save action.
        Task<ApiResponseDto<RadTrackInvoiceDto>> CreateAsync(RadTrackInvoiceDto dto);

        // TRANSFORMENGINE: PUT update — Edit Invoice modal save action.
        // id = InvoiceCounter PK, required to match backend route api/v1/radtrackinvoice/{id}.
        Task<ApiResponseDto<RadTrackInvoiceDto>> UpdateAsync(int id, RadTrackInvoiceDto dto);

        // TRANSFORMENGINE: DELETE — Delete Invoice confirmation dialog action.
        // Returns ApiResponseDto<object> wrapping { success: bool } backend response.
        Task<ApiResponseDto<object>> DeleteAsync(int id);
    }
}
