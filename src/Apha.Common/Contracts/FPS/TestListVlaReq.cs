/*
 * TRANSFORMENGINE MIGRATION — TestListVlaReq.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - New contract created from fps.testorproduct PostgreSQL table DDL and
 *     frmTestList / fsubTest_MainList ControlSource-bound writable fields
 *   - Composite PK (ItemCode, FpsYear) included — required for create/update routing
 *   - Writable user-input fields only; read-only computed/display-only fields excluded
 *   - money PostgreSQL type mapped to decimal?/decimal in C#
 *
 * PRESERVED:
 *   - All writable columns from fps.testorproduct: itemcode, itemdescription,
 *     testmanager, jobstatus, unitpricevla, priceahvg, owner, chargemethod,
 *     shortdescription, defraunitprice, fpsyear
 *   - Nullability aligned with DDL constraints
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Add [Required] / data-annotation validation attributes
 *     at Application layer once service interfaces are created (Phase 3).
 *   - TRANSFORMENGINE TODO: Confirm owner field allowed values (PT/PA/SD/LT) are
 *     validated in the service layer, not duplicated in this contract.
 */

namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Request contract for create/update operations on TestOrProduct VLA entries.
    /// Contains only writable ControlSource-bound fields from frmTestList / fsubTest_MainList.
    /// Route keys: ItemCode + FpsYear (composite PK on fps.testorproduct).
    /// </summary>
    public class TestListVlaReq
    {
        // TRANSFORMENGINE: Composite PK — both fields required for PUT /api/v1/testlistvla/{itemCode}/{fpsYear}
        public string ItemCode { get; set; } = null!;
        public int FpsYear { get; set; }

        public string? ItemDescription { get; set; }
        public string? TestManager { get; set; }
        public string? JobStatus { get; set; }

        // TRANSFORMENGINE: Writable pricing fields — PostgreSQL money → decimal?
        public decimal? UnitPriceVla { get; set; }
        public decimal? PriceAhvg { get; set; }

        // TRANSFORMENGINE: owner NOT NULL per CHECK constraint; nullable here to allow
        //   partial-update patterns — service must validate before persisting
        public string? Owner { get; set; }
        public string? ChargeMethod { get; set; }
        public string? ShortDescription { get; set; }

        // TRANSFORMENGINE: defraunitprice NOT NULL DEFAULT 0 — kept non-nullable in request
        public decimal DefraUnitPrice { get; set; }
    }
}
