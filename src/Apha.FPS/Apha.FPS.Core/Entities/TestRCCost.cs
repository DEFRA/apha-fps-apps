/*
 * TRANSFORMENGINE MIGRATION — TestRCCost.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - New entity class created from fps.tbltestrccost PostgreSQL table DDL
 *   - Composite PK (testcode, profitcentre, fpsyear) expressed as three properties;
 *     EF mapping via IEntityTypeConfiguration<T> handled in DataAccess layer
 *   - PostgreSQL money NOT NULL DEFAULT 0 column (price) mapped to non-nullable decimal
 *   - Table is partitioned by fpsyear in the database; entity is unaware of partitioning
 *   - FK constraints to fps.testorproduct and fps.tblkpprofitcentre preserved as comments;
 *     enforcement deferred to service/EF configuration layer
 *
 * PRESERVED:
 *   - All columns from fps.tbltestrccost: testcode, profitcentre, price, fpsyear
 *   - Nullability per DDL constraints (price is NOT NULL DEFAULT 0)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: FK fk_tbltestrccost_testcode — validate TestCode + FpsYear
 *     exists in fps.testorproduct at service layer before insert/update.
 *   - TRANSFORMENGINE TODO: FK fk_tbltestrccost_profitcentre — validate ProfitCentre
 *     exists in fps.tblkpprofitcentre at service layer before insert/update.
 *   - TRANSFORMENGINE TODO: PostgreSQL PARTITION BY LIST (fpsyear) — EF Core mapping
 *     must target the parent table fps.tbltestrccost, not a partition slice directly.
 */

namespace Apha.FPS.Core.Entities
{
    /// <summary>
    /// Entity representing a component charge per profit centre for a test/product.
    /// Maps to fps.tbltestrccost (composite PK: TestCode + ProfitCentre + FpsYear).
    /// The table is partitioned by fpsyear in PostgreSQL; EF maps to the parent table.
    /// </summary>
    public partial class TestRCCost
    {
        // TRANSFORMENGINE: Composite PK — (TestCode, ProfitCentre, FpsYear) per pk_tbltestrccost constraint
        public string TestCode { get; set; } = null!;

        // TRANSFORMENGINE: profitcentre character varying(50) NOT NULL — FK to fps.tblkpprofitcentre
        public string ProfitCentre { get; set; } = null!;

        public int FpsYear { get; set; }

        // TRANSFORMENGINE: price money NOT NULL DEFAULT 0 — non-nullable decimal
        public decimal Price { get; set; }
    }
}
