// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — RadTrackInvoiceTotalsDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-06-12
 *
 * CHANGED:
 *   - New file: application-layer DTO representing the totals footer row shown in
 *     the frmpimsinvoices HTML prototype.
 *   - Maps from RadTrackInvoiceTotals Core value object (co-located in
 *     Apha.PIMS.Core.Entities.RadTrackInvoice.cs).
 *   - Three aggregate sum properties: TotalPlannedAmount, TotalDueAmount, TotalActualAmount.
 *
 * PRESERVED:
 *   - Property names and types match the RadTrackInvoiceTotals Core value object to support
 *     convention AutoMapper mapping in EntityMapper.cs.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: If the frontend totals row requires formatted currency strings,
 *     add computed properties or a view-model adapter in the frontend layer rather than here.
 */

namespace Apha.PIMS.Application.Dtos
{
    // TRANSFORMENGINE: Totals footer DTO — aggregates PlannedAmount, DueAmount, and ActualAmount
    // across the current filtered invoice set. Returned by IRadTrackInvoiceService.GetTotalsAsync
    // and rendered as the bottom summary row in the invoice data grid.
    public class RadTrackInvoiceTotalsDto
    {
        // TRANSFORMENGINE: Sum of all PlannedAmount values for the current filter — "Planned" totals column.
        public double TotalPlannedAmount { get; set; }

        // TRANSFORMENGINE: Sum of all DueAmount values for the current filter — "Amount Due" totals column.
        public double TotalDueAmount { get; set; }

        // TRANSFORMENGINE: Sum of all ActualAmount values for the current filter — "Amount Invoiced" totals column.
        public double TotalActualAmount { get; set; }
    }
}
