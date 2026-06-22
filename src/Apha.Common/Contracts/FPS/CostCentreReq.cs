/*
 * TRANSFORMENGINE MIGRATION — CostCentreReq.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - MS Access frmMaintCostCentres ControlSource-bound fields → ASP.NET Core request contract
 *   - CostCentre TextBox (double) + ProfitCentre ComboBox (string) mapped to typed C# properties
 *   - Writable fields only: excludes FpsYear (partition key set server-side, not submitted by client)
 *
 * PRESERVED:
 *   - Field names and types aligned with fps.costcentre PostgreSQL table schema
 *   - ProfitCentre cardinality: nullable FK reference (character varying(50))
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Add [Required] / [Range] data annotation validators once
 *     the Application layer validation strategy is confirmed across FPS contracts.
 */

namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Request contract for CostCentre Create and Update operations.
    /// Contains only the writable ControlSource-bound fields from frmMaintCostCentres.
    /// Maps to the fps.costcentre table (costcentre, profitcentre columns).
    /// </summary>
    public class CostCentreReq
    {
        // TRANSFORMENGINE: maps to fps.costcentre.costcentre (double precision PK component)
        public double CostCentreNo { get; set; }

        // TRANSFORMENGINE: maps to fps.costcentre.profitcentre (FK → fps.tblkpprofitcentre, varchar 50)
        public string ProfitCentre { get; set; } = null!;
    }
}
