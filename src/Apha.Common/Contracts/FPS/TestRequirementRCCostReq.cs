/*
 * TRANSFORMENGINE MIGRATION — TestRequirementRCCostReq.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - New contract created from fps.tbltestrequirementrccost PostgreSQL table DDL and
 *     fsubTestequirementRCPrice ControlSource-bound writable fields
 *   - All columns are user-writable input fields (no computed/derived fields on this entity)
 *   - money NOT NULL → decimal in C# (non-nullable, matches DDL constraint)
 *
 * PRESERVED:
 *   - All writable columns from fps.tbltestrequirementrccost:
 *     testcode, buyer, profitcentre, price, fpsyear
 *   - Nullability aligned with DDL constraints
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Validate TestCode + Buyer + FpsYear exists in fps.tlkptestreqmt
 *     before insert — enforce at service layer (Phase 3).
 *   - TRANSFORMENGINE TODO: Validate TestCode + ProfitCentre + FpsYear exists in
 *     fps.tbltestrccost before insert — enforce at service layer (Phase 3).
 */

namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Request contract for create/update operations on project-specific component charges
    /// (TestRequirementRCCost).
    /// Route keys for update/delete: TestCode + Buyer + ProfitCentre + FpsYear
    /// (composite PK on fps.tbltestrequirementrccost).
    /// Consumed by POST /api/v1/testrequirementrccost and
    /// PUT /api/v1/testrequirementrccost/{testCode}/{buyer}/{profitCentre}/{fpsYear}.
    /// </summary>
    public class TestRequirementRCCostReq
    {
        // TRANSFORMENGINE: Composite PK — all four fields required for PUT/DELETE routing
        public string TestCode { get; set; } = null!;
        public string Buyer { get; set; } = null!;
        public string ProfitCentre { get; set; } = null!;
        public int FpsYear { get; set; }

        // TRANSFORMENGINE: price NOT NULL — non-nullable decimal matches DB constraint
        public decimal Price { get; set; }
    }
}
