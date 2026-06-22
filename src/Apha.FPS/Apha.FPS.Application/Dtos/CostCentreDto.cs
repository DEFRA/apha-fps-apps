/*
 * TRANSFORMENGINE MIGRATION — CostCentreDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - MS Access frmMaintCostCentres RecordSource columns (CostCentre, ProfitCentre) → application-layer DTO
 *   - Adds FpsYear to DTO surface (from composite PK; not present in VBA form but required for server-side year isolation)
 *   - CostCentreNo named to avoid class-name collision (mirrors CostCentre entity and CostCentreRes contract)
 *
 * PRESERVED:
 *   - All three table columns from fps.costcentre: costcentre (double) → CostCentreNo, profitcentre (varchar) → ProfitCentre, fpsyear (int) → FpsYear
 *   - Non-nullable constraints from DDL (null! initializer for string, no nullable annotations on value types)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: If additional computed/display fields are needed for the grid (e.g. ProfitCentreName from JOIN), add a separate CostCentreViewDto.
 */

namespace Apha.FPS.Application.Dtos
{
    // TRANSFORMENGINE: DTO mirrors fps.costcentre table surface — used as service-layer contract between Application and API layers
    public class CostCentreDto
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
        /// Part of composite PK; set server-side via FpsSetting context.
        /// </summary>
        public int FpsYear { get; set; }
    }
}
