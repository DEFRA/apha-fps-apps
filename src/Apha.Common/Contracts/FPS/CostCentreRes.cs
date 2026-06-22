/*
 * TRANSFORMENGINE MIGRATION — CostCentreRes.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - MS Access frmMaintCostCentres RecordSource fields → ASP.NET Core response contract
 *   - Full RecordSource surface: costcentre + profitcentre + fpsyear (partition key)
 *   - FpsYear included so frontend can scope requests and display context correctly
 *
 * PRESERVED:
 *   - Field names and types aligned with fps.costcentre PostgreSQL table schema
 *   - All three columns of the composite PK (CostCentreNo, FpsYear) surfaced for consumer routing
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated for Phase 1 scope.
 */

namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Response contract for CostCentre CRUD endpoints.
    /// Exposes the full RecordSource surface of fps.costcentre required by list, get, create, and update responses.
    /// FpsYear is included to provide the partition context needed by frontend consumers.
    /// </summary>
    public class CostCentreRes
    {
        // TRANSFORMENGINE: maps to fps.costcentre.costcentre (double precision, PK component 1)
        public double CostCentreNo { get; set; }

        // TRANSFORMENGINE: maps to fps.costcentre.profitcentre (FK → fps.tblkpprofitcentre, varchar 50)
        public string ProfitCentre { get; set; } = null!;

        // TRANSFORMENGINE: maps to fps.costcentre.fpsyear (PK component 2, partition key; FK → fps.tblyearmaster)
        public int FpsYear { get; set; }
    }
}
