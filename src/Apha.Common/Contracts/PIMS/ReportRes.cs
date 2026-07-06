/*
 * TRANSFORMENGINE MIGRATION — ReportRes.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - MS Access tblreport VBA/DAO form binding -> .NET 10 ASP.NET Core response contract
 *   - Full RecordSource surface from mabarchive.tblreport exposed for CRUD responses
 *   - PK (Id) included in Res for round-trip identity
 *
 * PRESERVED:
 *   - All column names and data types from mabarchive.tblreport DDL
 *   - NOT NULL constraints reflected as non-nullable properties
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: verify 'type' char(1) domain values with business owner
 */

namespace Apha.Common.Contracts.PIMS
{
    // TRANSFORMENGINE: full RecordSource surface — PK id included for list/detail responses
    public class ReportRes
    {
        public int Id { get; set; }
        public string ReportName { get; set; } = null!;
        public string? ReportDescription { get; set; }
        public string? Filter { get; set; }
        public string? MailComment { get; set; }
        public string? MailTitle { get; set; }
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
