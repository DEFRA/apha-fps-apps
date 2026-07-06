/*
 * TRANSFORMENGINE MIGRATION — SettingReq.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - MS Access tbl_settings VBA/DAO form binding -> .NET 10 ASP.NET Core request contract
 *   - Writable ControlSource fields from mabarchive.tbl_settings surfaced as public properties
 *   - PK (id varchar(50)) excluded from Req body; supplied via route on update
 *   - UserUpdateable flag preserved — controls whether end-users may update this setting
 *
 * PRESERVED:
 *   - All column names and data types from mabarchive.tbl_settings DDL
 *   - Nullable columns preserved as nullable properties
 *   - DEFAULT false on UserUpdateable reflected as non-nullable bool with sensible default
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm which fields are displayed vs hidden in the UI form (notes, testsetting)
 *   - TRANSFORMENGINE TODO: verify authorization rules — only admin roles should update settings
 */

namespace Apha.Common.Contracts.PIMS
{
    // TRANSFORMENGINE: writable fields only — id (natural PK varchar) excluded; supplied via route on update
    public class SettingReq
    {
        public string? Setting { get; set; }
        public string? Notes { get; set; }
        public string? TestSetting { get; set; }

        // TRANSFORMENGINE: DEFAULT false — non-nullable bool preserved from DDL
        public bool UserUpdateable { get; set; }
    }
}
