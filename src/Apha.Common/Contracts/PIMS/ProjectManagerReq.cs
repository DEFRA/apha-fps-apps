/*
 * TRANSFORMENGINE MIGRATION — ProjectManagerReq.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - MS Access tblprojectmanager VBA/DAO form binding -> .NET 10 ASP.NET Core request contract
 *   - Writable ControlSource fields from mabarchive.tblprojectmanager surfaced as public properties
 *   - PK (projectmanager varchar) included in Req — natural key, required on create; supplied via route on update
 *
 * PRESERVED:
 *   - All column names and data types from mabarchive.tblprojectmanager DDL
 *   - NOT NULL constraints reflected as non-nullable properties
 *   - DEFAULT false on Disable reflected as non-nullable bool
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm natural-key update strategy (rename scenario) with business owner
 */

namespace Apha.Common.Contracts.PIMS
{
    // TRANSFORMENGINE: natural PK (ProjectManager varchar) — included in Req for create; supplied via route on update
    public class ProjectManagerReq
    {
        public string ProjectManager { get; set; } = null!;
        public string? Email { get; set; }
        public string? MNumber { get; set; }

        // TRANSFORMENGINE: DEFAULT false NOT NULL — non-nullable bool preserved
        public bool Disable { get; set; }
    }
}
