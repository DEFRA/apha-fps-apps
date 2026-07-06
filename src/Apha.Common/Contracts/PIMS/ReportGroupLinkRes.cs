/*
 * TRANSFORMENGINE MIGRATION — ReportGroupLinkRes.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - MS Access tblreportgroup_link VBA/DAO form binding -> .NET 10 ASP.NET Core response contract
 *   - Full RecordSource surface from mabarchive.tblreportgroup_link exposed for CRUD responses
 *   - Both FK columns (ReportId, GroupId) included as they form the composite PK
 *
 * PRESERVED:
 *   - All column names and data types from mabarchive.tblreportgroup_link DDL
 *   - NOT NULL / PK constraints reflected as non-nullable properties
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: none — fully automated from DDL
 */

namespace Apha.Common.Contracts.PIMS
{
    // TRANSFORMENGINE: composite PK link table — both FK columns are response fields
    public class ReportGroupLinkRes
    {
        public int ReportId { get; set; }
        public int GroupId { get; set; }
    }
}
