// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — RadTrackInvoiceDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-06-12
 *
 * CHANGED:
 *   - New file: no prior DTO existed for the RadTrackInvoice entity.
 *   - All 11 entity fields carried forward as service-layer contract properties.
 *   - InvoiceCounter included to allow round-trip Edit/Delete flows (grid row identity).
 *   - Nullability mirrors the Core entity (Apha.PIMS.Core.Entities.RadTrackInvoice):
 *     InvoicePaid is non-nullable short (smallint NOT NULL DEFAULT 0); all others nullable.
 *
 * PRESERVED:
 *   - Field names and types match RadTrackInvoice entity exactly to enable AutoMapper
 *     convention mapping in EntityMapper.cs without manual ForMember overrides.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Evaluate whether InvoicePaid should be exposed as bool instead
 *     of short on the DTO surface. If changed, update EntityMapper mapping accordingly.
 */

using System;

namespace Apha.PIMS.Application.Dtos
{
    // TRANSFORMENGINE: Service-layer DTO for RadTrackInvoice — drives both grid row display
    // and the Add/Edit modal fields shown in source/ui/pims/frmpimsinvoices.html.
    public class RadTrackInvoiceDto
    {
        // TRANSFORMENGINE: PK — required for Edit/Delete identification.
        public int InvoiceCounter { get; set; }

        // TRANSFORMENGINE: FK to mabarchive.g_tlkpproject_radtrackdata (project).
        public string? Project { get; set; }

        // TRANSFORMENGINE: plannedamount column — "Planned Amount" grid column.
        public double? PlannedAmount { get; set; }

        // TRANSFORMENGINE: dueamount column — "Amount Due" grid column.
        public double? DueAmount { get; set; }

        // TRANSFORMENGINE: duedate column — "Date Due" grid column.
        public DateTime? DueDate { get; set; }

        // TRANSFORMENGINE: actualamount column — "Amount Invoiced" grid column.
        public double? ActualAmount { get; set; }

        // TRANSFORMENGINE: dateinvoiced column — "Date Invoiced" grid column.
        public DateTime? DateInvoiced { get; set; }

        // TRANSFORMENGINE: contract column — FK to mabarchive.tblradtrackcontract.
        public string? Contract { get; set; }

        // TRANSFORMENGINE: datejobsheetraised column — "Date JS Raised" grid column.
        public DateTime? DateJobsheetRaised { get; set; }

        // TRANSFORMENGINE: invoiceref column — "Invoice Ref" grid column.
        public string? InvoiceRef { get; set; }

        // TRANSFORMENGINE: invoicepaid column — "Paid?" grid column; smallint NOT NULL DEFAULT 0.
        // TRANSFORMENGINE TODO: Evaluate mapping to bool on the DTO surface — see checklist.
        public short InvoicePaid { get; set; }
    }
}
