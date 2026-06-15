// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — InvoiceTotalsItem.cs (new file)
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-06-12
 *
 * CHANGED:
 *   - New file: totals footer row model for the Invoice data grid.
 *   - Mirrors the three aggregate values shown in the frmpimsinvoices.html totals row:
 *       invTotalPlanned    → TotalPlannedAmount
 *       invTotalAmountDue  → TotalDueAmount
 *       invTotalAmountInvoiced → TotalActualAmount
 *   - Property names match RadTrackInvoiceTotalsDto exactly for AutoMapper convention mapping.
 *     Add CreateMap<InvoiceTotalsItem, RadTrackInvoiceTotalsDto>().ReverseMap() to
 *     PimsViewModelMapper.cs (see deferred note in that file from Phase 10).
 *   - Used by InvoiceController.GetInvoiceTotals() to populate the totals footer in
 *     the _DataGrid partial or a dedicated partial view for the totals row.
 *
 * PRESERVED:
 *   - All three totals from frmpimsinvoices.html footer preserved.
 *   - Double (non-nullable) types match RadTrackInvoiceTotalsDto.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Add CreateMap<InvoiceTotalsItem, RadTrackInvoiceTotalsDto>().ReverseMap()
 *     to PimsViewModelMapper.cs — this was noted as deferred in the Phase 10 mapper header.
 *   - TRANSFORMENGINE TODO: The totals row partial view must reference these three properties.
 *     Verify currency formatting is applied (GBP £ prefix) in the Razor view.
 */

using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    // TRANSFORMENGINE: Totals footer model — maps from RadTrackInvoiceTotalsDto.
    // Rendered in the inv-total-footer section of the Invoice Index view.
    public class InvoiceTotalsItem
    {
        // TRANSFORMENGINE: Sum of all PlannedAmount values for the current filter.
        // Matches frmpimsinvoices.html invTotalPlanned span.
        [Display(Name = "Total Planned Amount")]
        public double TotalPlannedAmount { get; set; }

        // TRANSFORMENGINE: Sum of all DueAmount values for the current filter.
        // Matches frmpimsinvoices.html invTotalAmountDue span.
        [Display(Name = "Total Amount Due")]
        public double TotalDueAmount { get; set; }

        // TRANSFORMENGINE: Sum of all ActualAmount (Amount Invoiced) values for the current filter.
        // Matches frmpimsinvoices.html invTotalAmountInvoiced span.
        [Display(Name = "Total Amount Invoiced")]
        public double TotalActualAmount { get; set; }
    }
}
