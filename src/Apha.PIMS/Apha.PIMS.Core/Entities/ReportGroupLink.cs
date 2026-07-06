/*
 * TRANSFORMENGINE MIGRATION — ReportGroupLink.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Core entity derived from PostgreSQL DDL mabarchive.tblreportgroup_link
 *   - Composite PK (reportid, groupid) — no surrogate key
 *   - FK: groupid references mabarchive.tblreportgroup(groupid)
 *
 * PRESERVED:
 *   - Column naming convention consistent with other composite-PK entities in the project
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: composite PK mapping (HasKey(e => new { e.Reportid, e.Groupid })) must be set in ReportGroupLinkMap.cs (Phase 4)
 */

namespace Apha.PIMS.Core.Entities
{
    // TRANSFORMENGINE: entity maps to mabarchive.tblreportgroup_link (PostgreSQL); composite PK (reportid, groupid)
    public partial class ReportGroupLink
    {
        public int Reportid { get; set; }

        public int Groupid { get; set; }
    }
}
