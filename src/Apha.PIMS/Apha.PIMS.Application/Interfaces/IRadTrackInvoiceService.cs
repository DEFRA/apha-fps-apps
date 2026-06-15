// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — IRadTrackInvoiceService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-06-12
 *
 * CHANGED:
 *   - New file: service interface orchestrating RadTrackInvoice CRUD + filtered list + totals.
 *   - GetAllAsync uses QueryParameters<RadTrackInvoiceFilter> (Application-layer wrapper around
 *     PaginationParameters) to carry the four filter dimensions from the frmpimsinvoices toolbar:
 *     Project, Contract, Year, Program.
 *   - GetTotalsAsync takes only the filter dimensions to return aggregate sums that match the
 *     currently filtered grid — consistent with how the totals row works in the HTML prototype.
 *   - CreateAsync / UpdateAsync accept and return RadTrackInvoiceDto to keep the API controller
 *     fully decoupled from Core entities.
 *   - DeleteAsync returns bool (true = deleted, false = not found) following the IMilestoneService
 *     pattern already established in this Application layer.
 *   - ExistsAsync exposed on the interface so the API controller can perform a pre-check
 *     without going through the full create/update path.
 *
 * PRESERVED:
 *   - Method signatures aligned with transform-plan.md Phase 3 specification and the
 *     IRadTrackInvoiceRepository method surface (Phase 2).
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm RadTrackInvoiceFilter namespace import is resolvable —
 *     filter class is defined in Apha.PIMS.Core.Interfaces; no Application-layer re-definition
 *     is needed as long as the Core assembly is referenced.
 */

using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Core.Interfaces;

namespace Apha.PIMS.Application.Interfaces
{
    // TRANSFORMENGINE: Service interface for RadTrackInvoice — consumed by the API controller
    // (Phase 5). Implementation is RadTrackInvoiceService (this Phase 3).
    public interface IRadTrackInvoiceService
    {
        // TRANSFORMENGINE: Paginated, filtered list — drives the frmpimsinvoices data grid.
        // QueryParameters<RadTrackInvoiceFilter> carries page/size/sort plus Project, Contract, Year, Program filters.
        Task<PaginatedResult<RadTrackInvoiceDto>> GetAllAsync(QueryParameters<RadTrackInvoiceFilter> parameters);

        // TRANSFORMENGINE: Single-record fetch by PK — used by the Edit and Delete modal open flows.
        Task<RadTrackInvoiceDto?> GetByIdAsync(int invoiceCounter);

        // TRANSFORMENGINE: Create a new invoice record; throws ArgumentException if required fields
        // are missing, throws InvalidOperationException on duplicate InvoiceRef within same Project+Contract.
        Task<RadTrackInvoiceDto> CreateAsync(RadTrackInvoiceDto dto);

        // TRANSFORMENGINE: Update an existing invoice record; throws KeyNotFoundException if not found,
        // ArgumentException for invalid data, InvalidOperationException on duplicate InvoiceRef conflict.
        Task<RadTrackInvoiceDto> UpdateAsync(RadTrackInvoiceDto dto);

        // TRANSFORMENGINE: Delete by InvoiceCounter; returns true if deleted, false if not found.
        Task<bool> DeleteAsync(int invoiceCounter);

        // TRANSFORMENGINE: Aggregate totals for the current filter — drives the totals footer row.
        // Filter must match the same RadTrackInvoiceFilter used in GetAllAsync for consistency.
        Task<RadTrackInvoiceTotalsDto> GetTotalsAsync(RadTrackInvoiceFilter? filter);

        // TRANSFORMENGINE: Existence check for duplicate InvoiceRef within the same Project+Contract scope.
        // excludeInvoiceCounter allows self-exclusion during Update.
        Task<bool> ExistsAsync(string? project, string? contract, string? invoiceRef, int? excludeInvoiceCounter = null);
    }
}
