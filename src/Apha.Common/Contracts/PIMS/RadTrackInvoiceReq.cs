// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — RadTrackInvoiceReq.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-06-12
 *
 * CHANGED:
 *   - New file: no legacy C# equivalent existed.
 *   - Writable modal form fields extracted from source/ui/pims/frmpimsinvoices.html (Add/Edit Invoice modal).
 *   - Field types derived from source/mssql/MabArchive/Tables/tblRadTrackInvoice.sql DDL.
 *   - InvoicePaid mapped as short (smallint, default 0) to preserve DB type; bool alias noted in comment.
 *   - Request contract contains ONLY writable ControlSource-bound fields; InvoiceCounter (PK/IDENTITY) excluded.
 *
 * PRESERVED:
 *   - All column names from tblRadTrackInvoice DDL retained verbatim.
 *   - Field nullability follows source DDL nullable/not-null constraints.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether InvoicePaid should be bool or short in the final API surface.
 *   - TRANSFORMENGINE TODO: Confirm Project field length constraint (varchar(20) from DDL).
 *   - TRANSFORMENGINE TODO: Confirm Contract field length constraint (varchar(10) from DDL).
 *   - TRANSFORMENGINE TODO: Confirm InvoiceRef field length constraint (varchar(50) from DDL).
 */

using System;

namespace Apha.Common.Contracts.PIMS
{
    // TRANSFORMENGINE: Req contract — writable fields only (no PK InvoiceCounter).
    // Source: Add/Edit modal in source/ui/pims/frmpimsinvoices.html and
    //         tblRadTrackInvoice DDL (source/mssql/MabArchive/Tables/tblRadTrackInvoice.sql).
    public class RadTrackInvoiceReq
    {
        // TRANSFORMENGINE: invModalProject → Project (varchar(20), FK to G_tlkpProject_RadTrackData.ParentProject)
        public string? Project { get; set; }

        // TRANSFORMENGINE: invModalContract → Contract (varchar(10), FK to tblRadtrackContract.Contract)
        public string? Contract { get; set; }

        // TRANSFORMENGINE: invModalPlannedAmt → PlannedAmount (float, nullable in DDL)
        public double? PlannedAmount { get; set; }

        // TRANSFORMENGINE: invModalAmountDue → DueAmount (float, nullable in DDL)
        public double? DueAmount { get; set; }

        // TRANSFORMENGINE: invModalDateDue → DueDate (datetime, nullable in DDL; DD/MM/YYYY in HTML hint)
        public DateTime? DueDate { get; set; }

        // TRANSFORMENGINE: invModalAmtInvoiced → ActualAmount (float, nullable in DDL)
        public double? ActualAmount { get; set; }

        // TRANSFORMENGINE: invModalDateJSRaised → DateJobsheetRaised (datetime, nullable in DDL; DD/MM/YYYY in HTML hint)
        public DateTime? DateJobsheetRaised { get; set; }

        // TRANSFORMENGINE: invModalInvoiceRef → InvoiceRef (varchar(50), nullable in DDL)
        public string? InvoiceRef { get; set; }

        // TRANSFORMENGINE: invModalPaid → InvoicePaid (smallint NOT NULL default 0; kept as short to match DDL type)
        // TRANSFORMENGINE TODO: Evaluate mapping to bool if the API surface prefers it.
        public short InvoicePaid { get; set; }

        // TRANSFORMENGINE: invModalDateInvoiced → DateInvoiced (datetime, nullable in DDL; DD/MM/YYYY in HTML hint)
        public DateTime? DateInvoiced { get; set; }
    }
}
