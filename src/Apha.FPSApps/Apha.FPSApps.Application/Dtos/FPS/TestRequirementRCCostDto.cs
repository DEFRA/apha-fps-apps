/*
 * TRANSFORMENGINE MIGRATION — TestRequirementRCCostDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - New frontend DTO created mirroring Apha.FPS.Application.Dtos.TestRequirementRCCostDto
 *   - Namespace is Apha.FPSApps.Application.Dtos.FPS (frontend application layer)
 *   - Same property names, types, and nullability as backend DTO — 1:1 mirror for ApiDtoMapper round-trip
 *   - Composite PK (TestCode + Buyer + ProfitCentre + FpsYear) preserved
 *   - price NOT NULL (no DEFAULT) → non-nullable decimal (matches backend DTO and TestRequirementRCCostRes contract)
 *
 * PRESERVED:
 *   - All 5 property names from backend TestRequirementRCCostDto / TestRequirementRCCostRes:
 *     TestCode, Buyer, ProfitCentre, FpsYear, Price
 *   - Nullability: TestCode/Buyer/ProfitCentre (required, null!), FpsYear (int), Price (decimal, NOT NULL)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: buyer FK to fps.tlkptestreqmt(testcode, buyer, fpsyear) must be
 *     enforced at service layer, not DTO.
 *   - TRANSFORMENGINE TODO: profitcentre FK to fps.tbltestrccost(testcode, profitcentre, fpsyear)
 *     must be enforced at service layer — valid TestRCCost row must exist before insert.
 */

namespace Apha.FPSApps.Application.Dtos.FPS
{
    /// <summary>
    /// Frontend DTO for project-specific component charges (TestRequirementRCCost).
    /// Mirrors Apha.FPS.Application.Dtos.TestRequirementRCCostDto for use in the frontend
    /// application and infrastructure layers.
    /// Maps to fps.tbltestrequirementrccost (composite PK: TestCode + Buyer + ProfitCentre + FpsYear).
    /// </summary>
    public class TestRequirementRCCostDto
    {
        // TRANSFORMENGINE: Composite PK fields — (TestCode, Buyer, ProfitCentre, FpsYear) matching fps.tbltestrequirementrccost pk
        public string TestCode { get; set; } = null!;
        public string Buyer { get; set; } = null!;
        public string ProfitCentre { get; set; } = null!;
        public int FpsYear { get; set; }

        // TRANSFORMENGINE: price NOT NULL (no DEFAULT) in DDL — non-nullable decimal
        public decimal Price { get; set; }
    }
}
