/*
 * TRANSFORMENGINE MIGRATION — Setting.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Core entity derived from PostgreSQL DDL mabarchive.tbl_settings
 *   - PK is a varchar(50) string identifier (id)
 *   - userupdateable boolean with DEFAULT false — mapped as non-nullable bool
 *
 * PRESERVED:
 *   - Column naming convention consistent with other entities in the project
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm table name mapping to "tbl_settings" (underscore-prefixed name) in SettingMap.cs (Phase 4)
 */

namespace Apha.PIMS.Core.Entities
{
    // TRANSFORMENGINE: entity maps to mabarchive.tbl_settings (PostgreSQL); string PK
    public partial class Setting
    {
        public string Id { get; set; } = null!;

        public string? SettingValue { get; set; }

        public string? Notes { get; set; }

        public string? Testsetting { get; set; }

        public bool Userupdateable { get; set; }
    }
}
