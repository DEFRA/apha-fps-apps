/*
 * TRANSFORMENGINE MIGRATION — AccessLevel.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Core entity derived from PostgreSQL DDL mabarchive.tblaccesslevels
 *   - Composite PK (systemid, accesslevelid)
 *   - FK: systemid references mabarchive.tblaccesssystems(systemid)
 *
 * PRESERVED:
 *   - Column naming convention consistent with other entities in the project
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: composite PK mapping (HasKey(e => new { e.Systemid, e.Accesslevelid })) must be set in AccessLevelMap.cs (Phase 4)
 */

namespace Apha.PIMS.Core.Entities
{
    // TRANSFORMENGINE: entity maps to mabarchive.tblaccesslevels (PostgreSQL); composite PK (systemid, accesslevelid)
    public partial class AccessLevel
    {
        public int Systemid { get; set; }

        public int Accesslevelid { get; set; }

        public string? Accesslevel { get; set; }
    }
}
