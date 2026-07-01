/*
 * TRANSFORMENGINE MIGRATION — TestRCCostDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - New DTO class created for internal service-layer contract on TestRCCost records
 *   - Mirrors TestRCCost entity fields (composite PK: TestCode + ProfitCentre + FpsYear) for AutoMapper round-trip
 *   - price money NOT NULL DEFAULT 0 -> non-nullable decimal (matches entity and TestRCCostRes/Req contracts)
 *   - FK constraints (testcode -> fps.testorproduct, profitcentre -> fps.tblkpprofitcentre) not enforced here
 *
 * PRESERVED:
 *   - All property names and nullability aligned with TestRCCost entity and TestRCCostRes/Req contracts
 *   - Composite PK fields: TestCode, ProfitCentre, FpsYear
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: FK validation (TestCode+FpsYear in fps.testorproduct,
 *     ProfitCentre in fps.tblkpprofitcentre) enforced at service layer, not DTO.
 */

namespace Apha.FPS.Application.Dtos
{
    /// <summary>
    /// Internal DTO for component charges per profit centre (TestRCCost).
    /// Used as the service-layer transfer object between repository and API controller.
    /// Maps to fps.tbltestrccost (composite PK: TestCode + ProfitCentre + FpsYear).
    /// </summary>
    public class TestRCCostDto
    {
        // TRANSFORMENGINE: Composite PK fields — (TestCode, ProfitCentre, FpsYear) matching fps.tbltestrccost pk
        public string TestCode { get; set; } = null!;
        public string ProfitCentre { get; set; } = null!;
        public int FpsYear { get; set; }

        // TRANSFORMENGINE: price NOT NULL DEFAULT 0 — non-nullable decimal
        public decimal Price { get; set; }
    }
}
