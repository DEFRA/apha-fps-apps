/*
 * TRANSFORMENGINE MIGRATION — ReportReq.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - MS Access tblreport VBA/DAO form binding -> .NET 10 ASP.NET Core request contract
 *   - Writable ControlSource fields from mabarchive.tblreport surfaced as public properties
 *   - PK (id) excluded from Req; supplied via route on update
 *
 * PRESERVED:
 *   - All column names and data types from mabarchive.tblreport DDL
 *   - NOT NULL constraints reflected as non-nullable properties
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: verify 'type' char(1) domain values with business owner
 *   - TRANSFORMENGINE TODO: confirm 'filter' varchar(200) syntax constraints if any
 */

namespace Apha.Common.Contracts.PIMS
{
    // TRANSFORMENGINE: writable fields only — id (PK) excluded; supplied via route on update
    public class ReportReq
    {
        public string ReportName { get; set; } = null!;
        public string? ReportDescription { get; set; }
        public string? Filter { get; set; }
        public string? MailComment { get; set; }
        public string? MailTitle { get; set; }

        // TRANSFORMENGINE: boolean NOT NULL columns preserved as bool
        public bool Emailable { get; set; }
        public int? SortOrder { get; set; }
        public bool AllowPickProgramme { get; set; }
        public bool AllowPickProject { get; set; }
        public bool AllowPickManager { get; set; }
        public bool AllowPickContract { get; set; }
        public bool AllowPickCustomer { get; set; }
        public bool AllowPickMonth { get; set; }
        public bool AllowPickFYear { get; set; }
        public string? ReportHelp { get; set; }

        // TRANSFORMENGINE: char(1) type column — single-character domain flag
        public string Type { get; set; } = null!;
    }
}
