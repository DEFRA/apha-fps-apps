/*
 * TRANSFORMENGINE MIGRATION — AccessSystem.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Core entity derived from PostgreSQL DDL mabarchive.tblaccesssystems
 *   - Single integer PK (systemid)
 *
 * PRESERVED:
 *   - Column naming convention consistent with other entities in the project
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
 */

namespace Apha.PIMS.Core.Entities
{
    // TRANSFORMENGINE: entity maps to mabarchive.tblaccesssystems (PostgreSQL)
    public partial class AccessSystem
    {
        public int Systemid { get; set; }

        public string Systemname { get; set; } = null!;
    }
}
