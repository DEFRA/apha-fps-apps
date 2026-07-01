/*
 * TRANSFORMENGINE MIGRATION — TestRequirementRCCostDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - New DTO class created for internal service-layer contract on TestRequirementRCCost records
 *   - Mirrors TestRequirementRCCost entity fields (composite PK: TestCode + Buyer + ProfitCentre + FpsYear) for AutoMapper round-trip
 *   - price money NOT NULL (no DEFAULT) -> non-nullable decimal (matches entity and Res/Req contracts)
 *   - FK constraints (testcode+buyer -> fps.tlkptestreqmt, testcode+profitcentre -> fps.tbltestrccost) not enforced here
 *
 * PRESERVED:
 *   - All property names and nullability aligned with TestRequirementRCCost entity and Res/Req contracts
 *   - Composite PK fields: TestCode, Buyer, ProfitCentre, FpsYear
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: FK validation (TestCode+Buyer+FpsYear in fps.tlkptestreqmt,
 *     TestCode+ProfitCentre+FpsYear in fps.tbltestrccost) enforced at service layer, not DTO.
 */

namespace Apha.FPS.Application.Dtos
{
    /// <summary>
    /// Internal DTO for project-specific component charges (TestRequirementRCCost).
    /// Used as the service-layer transfer object between repository and API controller.
    /// Maps to fps.tbltestrequirementrccost (composite PK: TestCode + Buyer + ProfitCentre + FpsYear).
    /// </summary>
    public class TestRequirementRCCostDto
    {
        // TRANSFORMENGINE: Composite PK fields — (TestCode, Buyer, ProfitCentre, FpsYear) matching fps.tbltestrequirementrccost pk
        public string TestCode { get; set; } = null!;
        public string Buyer { get; set; } = null!;
        public string ProfitCentre { get; set; } = null!;
        public int FpsYear { get; set; }

        // TRANSFORMENGINE: price NOT NULL (no DEFAULT) — non-nullable decimal
        public decimal Price { get; set; }
    }
}
