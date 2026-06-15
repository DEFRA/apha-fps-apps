// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — RadTrackInvoiceTotalsDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-06-12
 *
 * CHANGED:
 *   - New file: frontend DTO mirroring Apha.PIMS.Application.Dtos.RadTrackInvoiceTotalsDto.
 *   - Resides in Apha.FPSApps.Application.Dtos.PIMS namespace for use in frontend
 *     service and infrastructure layers.
 *   - Three aggregate sum properties matching the backend DTO exactly.
 *
 * PRESERVED:
 *   - Property names and types match backend RadTrackInvoiceTotalsDto exactly:
 *     TotalPlannedAmount, TotalDueAmount, TotalActualAmount (all double, non-nullable).
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: If the frontend totals row requires currency string formatting,
 *     add computed properties or use a view-model adapter in the MVC layer rather than here.
 *   - TRANSFORMENGINE TODO: Backend GET api/v1/radtrackinvoice/totals returns DTO directly
 *     (no RadTrackInvoiceTotalsRes contract exists). If a typed Res contract is added to
 *     Apha.Common.Contracts.PIMS, update PimsApiDtoMapper and this DTO's mapping accordingly.
 */

namespace Apha.FPSApps.Application.Dtos.PIMS
{
    // TRANSFORMENGINE: Frontend totals footer DTO — mirrors Apha.PIMS.Application.Dtos.RadTrackInvoiceTotalsDto.
    // Returned by IPimsRadTrackInvoiceApiClient.GetTotalsAsync and rendered as the summary
    // row at the bottom of the invoice data grid.
    public class RadTrackInvoiceTotalsDto
    {
        // TRANSFORMENGINE: Sum of all PlannedAmount values for the current filter.
        public double TotalPlannedAmount { get; set; }

        // TRANSFORMENGINE: Sum of all DueAmount values for the current filter.
        public double TotalDueAmount { get; set; }

        // TRANSFORMENGINE: Sum of all ActualAmount values for the current filter.
        public double TotalActualAmount { get; set; }
    }
}
