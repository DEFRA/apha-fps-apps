/*
 * TRANSFORMENGINE MIGRATION — ReviewItem.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Core entity derived from PostgreSQL DDL mabarchive.tlkpreviewitem
 *   - Single integer PK (itemid)
 *
 * PRESERVED:
 *   - Column naming convention consistent with other lookup entities in the project
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
 */

namespace Apha.PIMS.Core.Entities
{
    // TRANSFORMENGINE: entity maps to mabarchive.tlkpreviewitem (PostgreSQL); lookup/reference table
    public partial class ReviewItem
    {
        public int Itemid { get; set; }

        public string? Item { get; set; }
    }
}
