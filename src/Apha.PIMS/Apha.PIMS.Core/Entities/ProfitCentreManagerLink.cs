/*
 * TRANSFORMENGINE MIGRATION — ProfitCentreManagerLink.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Core entity derived from PostgreSQL DDL mabarchive.tblprofitcentre_manager_link
 *   - Composite PK (profitcentre, manager) — string composite key
 *
 * PRESERVED:
 *   - Column naming convention consistent with other composite-PK entities in the project
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: composite PK mapping (HasKey(e => new { e.Profitcentre, e.Manager })) must be set in ProfitCentreManagerLinkMap.cs (Phase 4)
 */

namespace Apha.PIMS.Core.Entities
{
    // TRANSFORMENGINE: entity maps to mabarchive.tblprofitcentre_manager_link (PostgreSQL); composite PK (profitcentre, manager)
    public partial class ProfitCentreManagerLink
    {
        public string Profitcentre { get; set; } = null!;

        public string Manager { get; set; } = null!;
    }
}
