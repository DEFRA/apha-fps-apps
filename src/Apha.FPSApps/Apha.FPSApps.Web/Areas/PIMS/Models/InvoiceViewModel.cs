// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — InvoiceViewModel.cs (new file)
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-06-12
 *
 * CHANGED:
 *   - New file: MVC ViewModel for the Invoice (frmpimsinvoices) page.
 *   - InvoicesGrid: DataGridConfig<InvoiceItem> — main invoice data grid.
 *   - Four explicit filter dropdown lists (ProjectList, ContractList, YearList, ProgramList)
 *     matching the four <select> elements outside the grid container in frmpimsinvoices.html:
 *     invFilterProject, invFilterSurvContract, invFilterYear, invFilterProgram.
 *   - Current filter scalar properties (FilterProject, FilterContract, FilterYear, FilterProgram)
 *     hold the active selections for round-trip binding and ExtraFilterMethod consumption.
 *   - InvoiceTotals: holds the aggregate footer row from GET api/v1/radtrackinvoice/totals.
 *   - AutoMapper convention mapping: InvoiceViewModel <-> RadTrackInvoiceDto
 *     (registered in PimsViewModelMapper.cs Phase 10).
 *
 * PRESERVED:
 *   - Property names match RadTrackInvoiceDto exactly where convention mapping applies.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: ContractList is populated from a lookup endpoint not yet confirmed.
 *     Phase 10 checklist notes "RadTrackContract lookup TBD". Stub with empty list until resolved.
 *   - TRANSFORMENGINE TODO: ProgramList source endpoint not confirmed. Stub with empty list.
 *   - TRANSFORMENGINE TODO: Verify YearList range (currently hardcoded to 3 years) matches
 *     requirements; may need dynamic population from a year lookup service.
 */

using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class InvoiceViewModel
    {
        // ── Active filter scalars (round-trip binding + ExtraFilterMethod) ──────

        // TRANSFORMENGINE: FilterProject bound to invFilterProject <select> in frmpimsinvoices.html.
        public string? FilterProject { get; set; }

        // TRANSFORMENGINE: FilterContract bound to invFilterSurvContract <select>.
        public string? FilterContract { get; set; }

        // TRANSFORMENGINE: FilterYear bound to invFilterYear <select> (int? matches backend year param).
        public int? FilterYear { get; set; }

        // TRANSFORMENGINE: FilterProgram bound to invFilterProgram <select>.
        public string? FilterProgram { get; set; }

        // ── Filter dropdown lists ────────────────────────────────────────────────

        // TRANSFORMENGINE: Project filter options — populated from IProjectListService.GetAllProjectsListAsync().
        public List<SelectListItem> ProjectList { get; set; } = [];

        // TRANSFORMENGINE: Contract filter options — STUB: lookup endpoint TBD (see deferred note).
        public List<SelectListItem> ContractList { get; set; } = [];

        // TRANSFORMENGINE: Year filter options — populated with a rolling year range in controller.
        public List<SelectListItem> YearList { get; set; } = [];

        // TRANSFORMENGINE: Program filter options — STUB: lookup endpoint TBD (see deferred note).
        public List<SelectListItem> ProgramList { get; set; } = [];

        // ── DataGrid ─────────────────────────────────────────────────────────────

        // TRANSFORMENGINE: InvoicesGrid — main invoice data grid.
        // NEVER leave as new() — always built explicitly in InvoiceController.Index().
        public DataGridConfig<InvoiceItem> InvoicesGrid { get; set; } = new();

        // ── Totals footer ────────────────────────────────────────────────────────

        // TRANSFORMENGINE: InvoiceTotals — aggregate footer row.
        // Populated from GET api/v1/radtrackinvoice/totals with same filter params.
        public RadTrackInvoiceTotalsDto? InvoiceTotals { get; set; }
    }
}
