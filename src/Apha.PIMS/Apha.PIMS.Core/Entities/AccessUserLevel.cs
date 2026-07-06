/*
 * TRANSFORMENGINE MIGRATION — AccessUserLevel.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Core entity derived from PostgreSQL DDL mabarchive.tblaccessusers_levels
 *   - Composite PK (systemid, ntlogin, accesslevelid) — three-column composite key
 *   - FK to tblaccesslevels(systemid, accesslevelid) and tblaccessusers(systemid, ntlogin)
 *
 * PRESERVED:
 *   - Column naming convention consistent with other entities in the project
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: three-column composite PK mapping (HasKey(e => new { e.Systemid, e.Ntlogin, e.Accesslevelid })) must be set in AccessUserLevelMap.cs (Phase 4)
 */

namespace Apha.PIMS.Core.Entities
{
    // TRANSFORMENGINE: entity maps to mabarchive.tblaccessusers_levels (PostgreSQL); composite PK (systemid, ntlogin, accesslevelid)
    public partial class AccessUserLevel
    {
        public int Systemid { get; set; }

        public string Ntlogin { get; set; } = null!;

        public int Accesslevelid { get; set; }
    }
}
