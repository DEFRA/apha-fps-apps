/*
 * TRANSFORMENGINE MIGRATION — TestRequirementRCCost.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - New entity class created from fps.tbltestrequirementrccost PostgreSQL table DDL
 *   - Composite PK (testcode, buyer, profitcentre, fpsyear) expressed as four properties;
 *     EF mapping via IEntityTypeConfiguration<T> handled in DataAccess layer
 *   - PostgreSQL money NOT NULL column (price, no DEFAULT 0) mapped to non-nullable decimal
 *   - Table is partitioned by fpsyear in the database; entity is unaware of partitioning
 *   - FK constraints to fps.tlkptestreqmt and fps.tbltestrccost preserved as comments;
 *     enforcement deferred to service/EF configuration layer
 *
 * PRESERVED:
 *   - All columns from fps.tbltestrequirementrccost: testcode, buyer, profitcentre, price, fpsyear
 *   - Nullability per DDL constraints (price is NOT NULL, no DEFAULT)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: FK fk_tbltestrequirementrccost_testcode_buyer — validate
 *     TestCode + Buyer + FpsYear exists in fps.tlkptestreqmt before insert/update.
 *   - TRANSFORMENGINE TODO: FK fk_tbltestrequirementrccost_testcode_profitcentre — validate
 *     TestCode + ProfitCentre + FpsYear exists in fps.tbltestrccost before insert/update.
 *   - TRANSFORMENGINE TODO: PostgreSQL PARTITION BY LIST (fpsyear) — EF Core mapping
 *     must target the parent table fps.tbltestrequirementrccost, not a partition slice.
 */

namespace Apha.FPS.Core.Entities
{
    /// <summary>
    /// Entity representing a project-specific component charge (per buyer/profit centre) for a test.
    /// Maps to fps.tbltestrequirementrccost
    /// (composite PK: TestCode + Buyer + ProfitCentre + FpsYear).
    /// The table is partitioned by fpsyear in PostgreSQL; EF maps to the parent table.
    /// </summary>
    public partial class TestRequirementRCCost
    {
        // TRANSFORMENGINE: Composite PK — (TestCode, Buyer, ProfitCentre, FpsYear)
        //   per pk_tbltestrequirementrccost constraint
        public string TestCode { get; set; } = null!;

        // TRANSFORMENGINE: buyer character varying(20) NOT NULL —
        //   FK to fps.tlkptestreqmt(testcode, buyer, fpsyear)
        public string Buyer { get; set; } = null!;

        // TRANSFORMENGINE: profitcentre character varying(50) NOT NULL —
        //   FK to fps.tbltestrccost(testcode, profitcentre, fpsyear)
        public string ProfitCentre { get; set; } = null!;

        public int FpsYear { get; set; }

        // TRANSFORMENGINE: price money NOT NULL (no DEFAULT) — non-nullable decimal
        public decimal Price { get; set; }
    }
}
