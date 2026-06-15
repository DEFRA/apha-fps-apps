// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — IRadTrackInvoiceRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination (Steps 2-3)
 * Migrated : 2026-06-12
 *
 * CHANGED:
 *   - New file: no prior C# repository interface existed for RadTrackInvoice.
 *   - Async-only signatures following ICommentRepository and IProjectListRepository patterns.
 *   - GetAllAsync uses PaginationParameters<RadTrackInvoiceFilter> to carry the four filter
 *     dimensions visible in the HTML prototype toolbar: Project, Contract, Year, Program.
 *   - GetTotalsAsync returns RadTrackInvoiceTotals — aggregate of PlannedAmount, DueAmount,
 *     ActualAmount sums across the same filtered set, matching the totals row in the UI prototype.
 *   - ExistsAsync provides AnyAsync-style duplicate guard before Add/Update.
 *   - No DbContext, EF Core, or infrastructure types imported — Core-clean rule enforced.
 *
 * PRESERVED:
 *   - CRUD method surface (GetAll, GetById, Create, Update, Delete) matches transform-plan.md Phase 2 spec.
 *   - Filter dimensions (Project, Contract, Year, Program) derived from qryInvoices.msaccsql WHERE clause
 *     and frmpimsinvoices.html toolbar dropdowns.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm Year filter type (int vs. short) against the actual report-year lookup
 *     table used in Phase 4 implementation.
 *   - TRANSFORMENGINE TODO: GetTotalsAsync filter parameter should match the same filter applied in
 *     GetAllAsync — verify the Application layer passes consistent filter objects in Phase 3 service impl.
 */

using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Pagination;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apha.PIMS.Core.Interfaces
{
    // TRANSFORMENGINE: Filter bag for invoice list queries — one property per toolbar dropdown in
    // source/ui/pims/frmpimsinvoices.html. Applied in GetAllAsync and GetTotalsAsync to keep
    // the filtered set consistent between the data grid and the totals row.
    public class RadTrackInvoiceFilter
    {
        // TRANSFORMENGINE: "Project" dropdown filter (maps to tblradtrackinvoice.project / g_tlkpproject_radtrackdata.parentproject).
        public string? Project { get; set; }

        // TRANSFORMENGINE: "Surveillance Contract" dropdown filter (maps to tblradtrackinvoice.contract).
        public string? Contract { get; set; }

        // TRANSFORMENGINE: "Year" dropdown filter — matches report year used in qryInvoices.msaccsql
        // (fnReportYear() equivalent). Nullable to indicate "all years".
        // TRANSFORMENGINE TODO: Confirm int vs. short — see checklist.
        public int? Year { get; set; }

        // TRANSFORMENGINE: "Program" dropdown filter — maps to g_tlkpproject_radtrackdata.program
        // (surfaced in qryInvoices.msaccsql as MY_tlkpProject.Program).
        public string? Program { get; set; }
    }

    // TRANSFORMENGINE: Repository interface for RadTrackInvoice CRUD + filtered list + aggregate totals.
    // Implementation lives in Apha.PIMS.DataAccess/Repository/RadTrackInvoiceRepository.cs (Phase 4).
    // Core layer must not reference DbContext or any EF/infrastructure namespace.
    public interface IRadTrackInvoiceRepository
    {
        // TRANSFORMENGINE: Paginated, filtered list — drives the data grid in frmpimsinvoices.html.
        // PaginationParameters<RadTrackInvoiceFilter> carries page/size/sort plus the four filter dimensions.
        Task<PagedData<RadTrackInvoice>> GetAllAsync(PaginationParameters<RadTrackInvoiceFilter> query);

        // TRANSFORMENGINE: Single-record fetch by PK (InvoiceCounter) — used by Edit and Delete flows.
        Task<RadTrackInvoice?> GetByIdAsync(int invoiceCounter);

        // TRANSFORMENGINE: Insert a new invoice record; returns the saved entity including generated InvoiceCounter.
        Task<RadTrackInvoice> CreateAsync(RadTrackInvoice entity);

        // TRANSFORMENGINE: Full update of an existing invoice by InvoiceCounter; returns updated entity.
        Task<RadTrackInvoice> UpdateAsync(RadTrackInvoice entity);

        // TRANSFORMENGINE: Delete by InvoiceCounter; returns true if row was removed, false if not found.
        Task<bool> DeleteAsync(int invoiceCounter);

        // TRANSFORMENGINE: Aggregate totals (PlannedAmount, DueAmount, ActualAmount sums) for the current
        // filtered set — drives the totals row at the bottom of the invoice grid.
        // Filter must match the same RadTrackInvoiceFilter used in GetAllAsync for consistency.
        Task<RadTrackInvoiceTotals> GetTotalsAsync(RadTrackInvoiceFilter? filter);

        // TRANSFORMENGINE: AnyAsync-style existence check — used before Add/Update to prevent
        // duplicate InvoiceRef entries within the same Project+Contract scope.
        // excludeInvoiceCounter allows self-exclusion during Update (so existing record doesn't block itself).
        Task<bool> ExistsAsync(string? project, string? contract, string? invoiceRef, int? excludeInvoiceCounter = null);
    }
}
