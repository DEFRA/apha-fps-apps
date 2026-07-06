/*
 * TRANSFORMENGINE MIGRATION — AccessUser.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Core entity derived from PostgreSQL DDL mabarchive.tblaccessusers
 *   - Composite PK (systemid, ntlogin)
 *   - FK: systemid references mabarchive.tblaccesssystems(systemid)
 *
 * PRESERVED:
 *   - Column naming convention consistent with other entities in the project
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: composite PK mapping (HasKey(e => new { e.Systemid, e.Ntlogin })) must be set in AccessUserMap.cs (Phase 4)
 */

namespace Apha.PIMS.Core.Entities
{
    // TRANSFORMENGINE: entity maps to mabarchive.tblaccessusers (PostgreSQL); composite PK (systemid, ntlogin)
    public partial class AccessUser
    {
        public int Systemid { get; set; }

        public string Ntlogin { get; set; } = null!;

        public string? Username { get; set; }

        public string? Dt2login { get; set; }
    }
}
