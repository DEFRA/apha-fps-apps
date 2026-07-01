/*
 * TRANSFORMENGINE MIGRATION — TestRCCostReq.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - New contract created from fps.tbltestrccost PostgreSQL table DDL and
 *     fsubTestRCPrice ControlSource-bound writable fields
 *   - All columns are user-writable input fields (no computed/derived fields on this entity)
 *   - money NOT NULL DEFAULT 0 → decimal in C# (non-nullable, reflects DB constraint)
 *
 * PRESERVED:
 *   - All writable columns from fps.tbltestrccost: testcode, profitcentre, price, fpsyear
 *   - Nullability aligned with DDL constraints
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Validate that TestCode + FpsYear exists in fps.testorproduct
 *     before inserting — enforce at service layer (Phase 3).
 *   - TRANSFORMENGINE TODO: Validate ProfitCentre FK to fps.tblkpprofitcentre at service layer.
 */

namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Request contract for create/update operations on component charges per profit centre (TestRCCost).
    /// Route keys for update/delete: TestCode + ProfitCentre + FpsYear
    /// (composite PK on fps.tbltestrccost).
    /// Consumed by POST /api/v1/testrccost and PUT /api/v1/testrccost/{testCode}/{profitCentre}/{fpsYear}.
    /// </summary>
    public class TestRCCostReq
    {
        // TRANSFORMENGINE: Composite PK — all three fields required for PUT/DELETE routing
        public string TestCode { get; set; } = null!;
        public string ProfitCentre { get; set; } = null!;
        public int FpsYear { get; set; }

        // TRANSFORMENGINE: price NOT NULL DEFAULT 0 — non-nullable decimal matches DB constraint
        public decimal Price { get; set; }
    }
}
