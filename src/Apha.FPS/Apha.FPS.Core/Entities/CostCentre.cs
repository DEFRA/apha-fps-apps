/*
 * TRANSFORMENGINE MIGRATION — CostCentre.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - MS Access frmMaintCostCentres RecordSource (fps.costcentre table) → EF Core entity class
 *   - Column "costcentre double precision" → CostCentreNo (double) to avoid name clash with class name
 *   - Column "profitcentre varchar(50)" → ProfitCentre (string, non-nullable)
 *   - Column "fpsyear integer" → FpsYear (int); part of composite PK (CostCentreNo, FpsYear)
 *   - FK references fps.tblyearmaster (FpsYear) and fps.tblkpprofitcentre (ProfitCentre) preserved as nav-property placeholders
 *
 * PRESERVED:
 *   - All column nullability constraints from DDL: costcentre NOT NULL, profitcentre NOT NULL, fpsyear NOT NULL
 *   - Composite primary key semantics (CostCentreNo, FpsYear) — enforced in CostCentreMap (Phase 4)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Navigation properties to ProfitCentre and YearMaster not added here; add if EF lazy-loading navigation is required downstream.
 */

namespace Apha.FPS.Core.Entities
{
    // TRANSFORMENGINE: fps.costcentre table entity — composite PK (CostCentreNo, FpsYear) mapped in CostCentreMap (Phase 4)
    public partial class CostCentre
    {
        /// <summary>
        /// Cost centre number. Maps to DB column "costcentre" (double precision NOT NULL).
        /// Named CostCentreNo to avoid collision with the class name.
        /// </summary>
        public double CostCentreNo { get; set; }

        /// <summary>
        /// Profit centre code. Maps to DB column "profitcentre" (varchar(50) NOT NULL).
        /// FK → fps.tblkpprofitcentre.
        /// </summary>
        public string ProfitCentre { get; set; } = null!;

        /// <summary>
        /// FPS financial year. Maps to DB column "fpsyear" (integer NOT NULL).
        /// Part of composite PK; FK → fps.tblyearmaster.
        /// </summary>
        public int FpsYear { get; set; }
    }
}
