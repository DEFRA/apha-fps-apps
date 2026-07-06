/*
 * TRANSFORMENGINE MIGRATION — ProgramManagerLink.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Core entity derived from PostgreSQL DDL mabarchive.tblprogram_manager_link
 *   - Composite PK (program, manager) — string composite key
 *
 * PRESERVED:
 *   - Column naming convention consistent with other composite-PK entities in the project
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: composite PK mapping (HasKey(e => new { e.Program, e.Manager })) must be set in ProgramManagerLinkMap.cs (Phase 4)
 */

namespace Apha.PIMS.Core.Entities
{
    // TRANSFORMENGINE: entity maps to mabarchive.tblprogram_manager_link (PostgreSQL); composite PK (program, manager)
    public partial class ProgramManagerLink
    {
        public string Program { get; set; } = null!;

        public string Manager { get; set; } = null!;
    }
}
