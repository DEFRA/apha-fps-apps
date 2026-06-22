/*
 * TRANSFORMENGINE MIGRATION — CostCentreDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - New frontend DTO mirroring Apha.FPS.Application.Dtos.CostCentreDto (backend application layer)
 *   - Lives in Apha.FPSApps.Application.Dtos.FPS namespace (frontend application layer)
 *   - Used by IFpsCostCentreApiClient for create/update/get-by-id operations
 *
 * PRESERVED:
 *   - Exact property names from backend DTO: CostCentreNo (double), ProfitCentre (string), FpsYear (int)
 *   - Non-nullable string initializer (null!) matching backend DTO convention
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: If a grid projection with JOIN fields (e.g. ProfitCentreName) is needed, add a separate CostCentreViewDto — currently the DataGrid can be served by this DTO as the backend does not expose a separate view DTO.
 */

namespace Apha.FPSApps.Application.Dtos.FPS
{
    // TRANSFORMENGINE: frontend DTO mirrors backend Apha.FPS.Application.Dtos.CostCentreDto — same property names, different namespace
    public class CostCentreDto
    {
        /// <summary>
        /// Cost centre number. Mirrors backend CostCentreDto.CostCentreNo (double precision NOT NULL).
        /// Named CostCentreNo to avoid collision with the class name.
        /// </summary>
        public double CostCentreNo { get; set; }

        /// <summary>
        /// Profit centre code. Mirrors backend CostCentreDto.ProfitCentre (varchar NOT NULL).
        /// FK → fps.tblkpprofitcentre.
        /// </summary>
        public string ProfitCentre { get; set; } = null!;

        /// <summary>
        /// FPS financial year. Mirrors backend CostCentreDto.FpsYear (integer NOT NULL).
        /// Part of composite PK; set server-side via X-FPS-Year header / request context.
        /// </summary>
        public int FpsYear { get; set; }
    }
}
