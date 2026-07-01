/*
 * TRANSFORMENGINE MIGRATION — TestRCCostRes.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - New contract created from fps.tbltestrccost PostgreSQL table DDL
 *   - Composite PK (testcode, profitcentre, fpsyear) surfaced as response fields
 *   - money NOT NULL DEFAULT 0 → decimal in C# (non-nullable)
 *
 * PRESERVED:
 *   - All columns from fps.tbltestrccost: testcode, profitcentre, price, fpsyear
 *   - Nullability matches DDL constraints (price is NOT NULL)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Verify profitcentre FK constraint to fps.tblkpprofitcentre
 *     is enforced at service layer; not duplicated in this contract.
 */

namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Response contract for a component charge per profit centre (TestRCCost).
    /// Maps to fps.tbltestrccost (composite PK: TestCode + ProfitCentre + FpsYear).
    /// Consumed by GET /api/v1/testrccost/{testCode}/{fpsYear}.
    /// </summary>
    public class TestRCCostRes
    {
        // TRANSFORMENGINE: Composite PK fields — all three required for route resolution
        public string TestCode { get; set; } = null!;
        public string ProfitCentre { get; set; } = null!;
        public int FpsYear { get; set; }

        // TRANSFORMENGINE: price is NOT NULL DEFAULT 0 in DDL — non-nullable decimal
        public decimal Price { get; set; }
    }
}
