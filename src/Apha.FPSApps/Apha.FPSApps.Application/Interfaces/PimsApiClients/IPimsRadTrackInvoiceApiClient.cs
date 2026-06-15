// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — IPimsRadTrackInvoiceApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-06-12
 *
 * CHANGED:
 *   - New file: typed API client interface for the frontend to call the backend
 *     RadTrackInvoice REST API at route api/v1/radtrackinvoice.
 *   - Methods mirror the 6 backend controller actions confirmed in Phase 5/6:
 *       GetAllAsync        -> GET  api/v1/radtrackinvoice  (paged + filtered)
 *       GetTotalsAsync     -> GET  api/v1/radtrackinvoice/totals
 *       GetByIdAsync       -> GET  api/v1/radtrackinvoice/{id}
 *       CreateAsync        -> POST api/v1/radtrackinvoice
 *       UpdateAsync        -> PUT  api/v1/radtrackinvoice/{id}
 *       DeleteAsync        -> DELETE api/v1/radtrackinvoice/{id}
 *   - Filter dimensions (project, contract, year, program) are exposed as explicit
 *     nullable parameters on GetAllAsync and GetTotalsAsync, matching the four toolbar
 *     dropdowns in source/ui/pims/frmpimsinvoices.html and the RadTrackInvoiceFilter
 *     bag defined in Apha.PIMS.Core.Interfaces.
 *
 * PRESERVED:
 *   - All return types wrapped in ApiResponseDto<T> per frontend convention.
 *   - GetTotalsAsync returns RadTrackInvoiceTotalsDto directly (no typed Res contract
 *     exists on the backend for totals; see backend DEFERRED note).
 *   - DeleteAsync returns ApiResponseDto<object> matching the backend anonymous
 *     { success: bool } response and IPimsMilestoneApiClient.DeleteMilestoneAsync pattern.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm query-string binding format for filter parameters:
 *     backend uses QueryParameters<RadTrackInvoiceFilter> — verify whether the HTTP client
 *     implementation should send ?filter.project=... (nested) or ?project=... (flat).
 *     Adjust PimsRadTrackInvoiceApiClient.cs accordingly after Phase 9.
 *   - TRANSFORMENGINE TODO: If RadTrackInvoiceTotalsRes contract is added to
 *     Apha.Common.Contracts.PIMS, update GetTotalsAsync return type to
 *     ApiResponseDto<RadTrackInvoiceTotalsDto> via PimsApiDtoMapper mapping.
 *   - TRANSFORMENGINE TODO: Verify year filter type: backend RadTrackInvoiceFilter.Year
 *     is int? — confirm whether the MVC page supplies an int or string, and adjust
 *     the parameter type here and in PimsRadTrackInvoiceApiClient.cs if needed.
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    public interface IPimsRadTrackInvoiceApiClient
    {
        // TRANSFORMENGINE: GET list — paged + filtered invoice grid.
        // Filter params map to frmpimsinvoices.html toolbar dropdowns:
        // Project dropdown, Surveillance Contract dropdown, Year dropdown, Program dropdown.
        Task<ApiResponseDto<List<RadTrackInvoiceDto>>> GetAllAsync(
            QueryParameters<string> query,
            string? project = null,
            string? contract = null,
            int? year = null,
            string? program = null);

        // TRANSFORMENGINE: GET totals — aggregate footer row matching the current filter.
        // Accepts the same filter dimensions as GetAllAsync.
        Task<ApiResponseDto<RadTrackInvoiceTotalsDto>> GetTotalsAsync(
            string? project = null,
            string? contract = null,
            int? year = null,
            string? program = null);

        // TRANSFORMENGINE: GET by PK — used by Edit and Delete modal open flows.
        Task<ApiResponseDto<RadTrackInvoiceDto>> GetByIdAsync(int id);

        // TRANSFORMENGINE: POST create — Add Invoice modal save action.
        Task<ApiResponseDto<RadTrackInvoiceDto>> CreateAsync(RadTrackInvoiceDto dto);

        // TRANSFORMENGINE: PUT update — Edit Invoice modal save action. Route id = InvoiceCounter.
        Task<ApiResponseDto<RadTrackInvoiceDto>> UpdateAsync(int id, RadTrackInvoiceDto dto);

        // TRANSFORMENGINE: DELETE — Delete Invoice confirmation dialog action.
        // Returns ApiResponseDto<object> wrapping { success: bool } response.
        Task<ApiResponseDto<object>> DeleteAsync(int id);
    }
}
