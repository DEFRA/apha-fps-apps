/*
 * TRANSFORMENGINE MIGRATION — ProfitCentreManagerLinkRes.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - MS Access tblprofitcentre_manager_link VBA/DAO form binding -> .NET 10 ASP.NET Core response contract
 *   - Full RecordSource surface from mabarchive.tblprofitcentre_manager_link exposed for CRUD responses
 *   - Both PK columns (ProfitCentre, Manager) included for round-trip identity
 *
 * PRESERVED:
 *   - All column names and data types from mabarchive.tblprofitcentre_manager_link DDL
 *   - NOT NULL / PK constraints reflected as non-nullable properties
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: none — fully automated from DDL
 */

namespace Apha.Common.Contracts.PIMS
{
    // TRANSFORMENGINE: composite natural PK link table — both columns are response fields
    public class ProfitCentreManagerLinkRes
    {
        public string ProfitCentre { get; set; } = null!;
        public string Manager { get; set; } = null!;
    }
}
