/*
 * TRANSFORMENGINE MIGRATION — ReportGroupLinkReq.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - MS Access tblreportgroup_link VBA/DAO form binding -> .NET 10 ASP.NET Core request contract
 *   - Writable ControlSource fields from mabarchive.tblreportgroup_link surfaced as public properties
 *   - Composite PK (reportid, groupid) included in Req — both are required to create or delete a link
 *
 * PRESERVED:
 *   - All column names and data types from mabarchive.tblreportgroup_link DDL
 *   - NOT NULL / PK constraints reflected as non-nullable properties
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm delete endpoint uses route params vs body binding for composite PK
 */

namespace Apha.Common.Contracts.PIMS
{
    // TRANSFORMENGINE: composite PK link table — both FK columns are writable inputs
    public class ReportGroupLinkReq
    {
        public int ReportId { get; set; }
        public int GroupId { get; set; }
    }
}
