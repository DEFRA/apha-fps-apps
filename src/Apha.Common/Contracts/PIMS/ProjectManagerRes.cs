/*
 * TRANSFORMENGINE MIGRATION — ProjectManagerRes.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - MS Access tblprojectmanager VBA/DAO form binding -> .NET 10 ASP.NET Core response contract
 *   - Full RecordSource surface from mabarchive.tblprojectmanager exposed for CRUD responses
 *   - PK (ProjectManager) included in Res for round-trip identity
 *
 * PRESERVED:
 *   - All column names and data types from mabarchive.tblprojectmanager DDL
 *   - NOT NULL constraints reflected as non-nullable properties
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: none — fully automated from DDL
 */

namespace Apha.Common.Contracts.PIMS
{
    // TRANSFORMENGINE: full RecordSource surface — natural PK (ProjectManager) included for list/detail responses
    public class ProjectManagerRes
    {
        public string ProjectManager { get; set; } = null!;
        public string? Email { get; set; }
        public string? MNumber { get; set; }
        public bool Disable { get; set; }
    }
}
