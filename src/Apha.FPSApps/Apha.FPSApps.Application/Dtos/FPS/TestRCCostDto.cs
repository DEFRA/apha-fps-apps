/*
 * TRANSFORMENGINE MIGRATION — TestRCCostDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - New frontend DTO created mirroring Apha.FPS.Application.Dtos.TestRCCostDto
 *   - Namespace is Apha.FPSApps.Application.Dtos.FPS (frontend application layer)
 *   - Same property names, types, and nullability as backend DTO — 1:1 mirror for ApiDtoMapper round-trip
 *   - Composite PK (TestCode + ProfitCentre + FpsYear) preserved
 *   - price NOT NULL DEFAULT 0 → non-nullable decimal (matches backend DTO and TestRCCostRes contract)
 *
 * PRESERVED:
 *   - All 4 property names from backend TestRCCostDto / TestRCCostRes:
 *     TestCode, ProfitCentre, FpsYear, Price
 *   - Nullability: TestCode/ProfitCentre (required, null!), FpsYear (int), Price (decimal, NOT NULL)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: FK validation (TestCode+FpsYear in fps.testorproduct,
 *     ProfitCentre in fps.tblkpprofitcentre) is enforced at service layer, not DTO.
 */

namespace Apha.FPSApps.Application.Dtos.FPS
{
    /// <summary>
    /// Frontend DTO for component charges per profit centre (TestRCCost).
    /// Mirrors Apha.FPS.Application.Dtos.TestRCCostDto for use in the frontend
    /// application and infrastructure layers.
    /// Maps to fps.tbltestrccost (composite PK: TestCode + ProfitCentre + FpsYear).
    /// </summary>
    public class TestRCCostDto
    {
        // TRANSFORMENGINE: Composite PK fields — (TestCode, ProfitCentre, FpsYear) matching fps.tbltestrccost pk
        public string TestCode { get; set; } = null!;
        public string ProfitCentre { get; set; } = null!;
        public int FpsYear { get; set; }

        // TRANSFORMENGINE: price NOT NULL DEFAULT 0 in DDL — non-nullable decimal
        public decimal Price { get; set; }
    }
}
