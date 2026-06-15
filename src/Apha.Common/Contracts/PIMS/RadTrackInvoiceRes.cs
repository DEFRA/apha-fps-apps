// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — RadTrackInvoiceRes.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-06-12
 *
 * CHANGED:
 *   - New file: no legacy C# equivalent existed.
 *   - Full RecordSource surface for CRUD responses, mirroring all tblRadTrackInvoice columns.
 *   - InvoiceCounter (PK/IDENTITY) included as the identifier needed by Edit/Delete operations and grid binding.
 *   - Grid columns from HTML prototype (frmpimsinvoices.html): Project, Contract, PlannedAmount, DueAmount,
 *     DueDate, ActualAmount, DateJobsheetRaised, InvoiceRef, InvoicePaid, DateInvoiced, InvoiceCounter.
 *   - Field types derived from source/mssql/MabArchive/Tables/tblRadTrackInvoice.sql DDL.
 *
 * PRESERVED:
 *   - All column names from tblRadTrackInvoice DDL retained verbatim.
 *   - Field nullability follows source DDL nullable/not-null constraints.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether InvoicePaid should be bool or short in the final API surface.
 *   - TRANSFORMENGINE TODO: Confirm Project, Contract, InvoiceRef display formatting requirements.
 */

using System;

namespace Apha.Common.Contracts.PIMS
{
    // TRANSFORMENGINE: Res contract — full RecordSource surface including PK InvoiceCounter for grid/CRUD binding.
    // Source: grid columns in source/ui/pims/frmpimsinvoices.html and
    //         tblRadTrackInvoice DDL (source/mssql/MabArchive/Tables/tblRadTrackInvoice.sql).
    public class RadTrackInvoiceRes
    {
        // TRANSFORMENGINE: InvoiceCounter — PK IDENTITY; included in Res for Edit/Delete row identification.
        public int InvoiceCounter { get; set; }

        // TRANSFORMENGINE: Grid column "Project" → Project (varchar(20), FK to G_tlkpProject_RadTrackData)
        public string? Project { get; set; }

        // TRANSFORMENGINE: Grid column "Contract" → Contract (varchar(10), FK to tblRadtrackContract)
        public string? Contract { get; set; }

        // TRANSFORMENGINE: Grid column "Planned Amount" → PlannedAmount (float, nullable in DDL)
        public double? PlannedAmount { get; set; }

        // TRANSFORMENGINE: Grid column "Amount Due" → DueAmount (float, nullable in DDL)
        public double? DueAmount { get; set; }

        // TRANSFORMENGINE: Grid column "Date Due" → DueDate (datetime, nullable in DDL; DD/MM/YYYY display)
        public DateTime? DueDate { get; set; }

        // TRANSFORMENGINE: Grid column "Amount Invoiced" → ActualAmount (float, nullable in DDL)
        public double? ActualAmount { get; set; }

        // TRANSFORMENGINE: Grid column "Date JS Raised" → DateJobsheetRaised (datetime, nullable in DDL; DD/MM/YYYY display)
        public DateTime? DateJobsheetRaised { get; set; }

        // TRANSFORMENGINE: Grid column "Invoice Ref" → InvoiceRef (varchar(50), nullable in DDL)
        public string? InvoiceRef { get; set; }

        // TRANSFORMENGINE: Grid column "Paid?" → InvoicePaid (smallint NOT NULL default 0; kept as short to match DDL type)
        // TRANSFORMENGINE TODO: Evaluate mapping to bool if the API surface prefers it.
        public short InvoicePaid { get; set; }

        // TRANSFORMENGINE: Grid column "Date Invoiced" → DateInvoiced (datetime, nullable in DDL; DD/MM/YYYY display)
        public DateTime? DateInvoiced { get; set; }
    }
}
