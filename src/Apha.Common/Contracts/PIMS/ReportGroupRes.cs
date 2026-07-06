/*
 * TRANSFORMENGINE MIGRATION — ReportGroupRes.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - MS Access tblreportgroup VBA/DAO form binding -> .NET 10 ASP.NET Core response contract
 *   - Full RecordSource surface from mabarchive.tblreportgroup exposed for CRUD and lookup responses
 *   - PK (GroupId) included in Res for round-trip identity and dropdown binding
 *
 * PRESERVED:
 *   - All column names and data types from mabarchive.tblreportgroup DDL
 *   - NOT NULL constraints reflected as non-nullable properties
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: none — fully automated from DDL
 */

namespace Apha.Common.Contracts.PIMS
{
    // TRANSFORMENGINE: full RecordSource surface — GroupId (IDENTITY PK) included for list/lookup responses
    public class ReportGroupRes
    {
        public int GroupId { get; set; }
        public string Description { get; set; } = null!;
    }
}
