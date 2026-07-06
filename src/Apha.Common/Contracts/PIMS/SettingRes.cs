/*
 * TRANSFORMENGINE MIGRATION — SettingRes.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - MS Access tbl_settings VBA/DAO form binding -> .NET 10 ASP.NET Core response contract
 *   - Full RecordSource surface from mabarchive.tbl_settings exposed for CRUD responses
 *   - PK (Id varchar(50)) included in Res for round-trip identity
 *
 * PRESERVED:
 *   - All column names and data types from mabarchive.tbl_settings DDL
 *   - Nullable columns preserved as nullable properties
 *   - DEFAULT false on UserUpdateable reflected as non-nullable bool
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm which fields are displayed vs hidden in the UI form (Notes, TestSetting)
 *   - TRANSFORMENGINE TODO: verify authorization rules — only admin roles should view/update settings
 */

namespace Apha.Common.Contracts.PIMS
{
    // TRANSFORMENGINE: full RecordSource surface — PK Id (varchar natural key) included for list/detail responses
    public class SettingRes
    {
        // TRANSFORMENGINE: id is varchar(50) natural PK — included in response for round-trip identity
        public string Id { get; set; } = null!;
        public string? Setting { get; set; }
        public string? Notes { get; set; }
        public string? TestSetting { get; set; }

        // TRANSFORMENGINE: DEFAULT false — non-nullable bool preserved from DDL
        public bool UserUpdateable { get; set; }
    }
}
