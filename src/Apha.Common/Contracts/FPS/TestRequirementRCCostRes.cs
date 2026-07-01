/*
 * TRANSFORMENGINE MIGRATION — TestRequirementRCCostRes.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - New contract created from fps.tbltestrequirementrccost PostgreSQL table DDL
 *   - Composite PK (testcode, buyer, profitcentre, fpsyear) surfaced as response fields
 *   - money NOT NULL → decimal in C# (non-nullable, no DEFAULT 0 in DDL but NOT NULL enforced)
 *
 * PRESERVED:
 *   - All columns from fps.tbltestrequirementrccost: testcode, buyer, profitcentre, price, fpsyear
 *   - Nullability matches DDL constraints (price is NOT NULL)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: buyer FK to fps.tlkptestreqmt(testcode, buyer, fpsyear) must be
 *     enforced at service layer, not contract layer.
 *   - TRANSFORMENGINE TODO: profitcentre FK to fps.tbltestrccost(testcode, profitcentre, fpsyear)
 *     must be enforced at service layer — a valid TestRCCost row must exist before insert.
 */

namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Response contract for a project-specific component charge (TestRequirementRCCost).
    /// Maps to fps.tbltestrequirementrccost
    /// (composite PK: TestCode + Buyer + ProfitCentre + FpsYear).
    /// Consumed by GET /api/v1/testrequirementrccost/{testCode}/{fpsYear}.
    /// </summary>
    public class TestRequirementRCCostRes
    {
        // TRANSFORMENGINE: Composite PK fields — all four required for route resolution and FK integrity
        public string TestCode { get; set; } = null!;
        public string Buyer { get; set; } = null!;
        public string ProfitCentre { get; set; } = null!;
        public int FpsYear { get; set; }

        // TRANSFORMENGINE: price is NOT NULL in DDL — non-nullable decimal in response
        public decimal Price { get; set; }
    }
}
