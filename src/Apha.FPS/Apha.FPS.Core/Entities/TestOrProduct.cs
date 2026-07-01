/*
 * TRANSFORMENGINE MIGRATION — TestOrProduct.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - New entity class created from fps.testorproduct PostgreSQL table DDL
 *   - Composite PK (itemcode, fpsyear) expressed as two properties; EF mapping handled in DataAccess layer
 *   - PostgreSQL money columns (unitpricevla, priceahvg, defraunitprice) mapped to decimal? / decimal
 *   - character varying and character column types mapped to string / string?
 *   - Table is partitioned by fpsyear in the database; entity is unaware of partitioning (EF handles via table name)
 *   - owner CHECK constraint (PT/PA/SD/LT) preserved as comment; enforcement deferred to service/validation layer
 *
 * PRESERVED:
 *   - All column names from fps.testorproduct: itemcode, itemdescription, testmanager,
 *     jobstatus, unitpricevla, priceahvg, owner, chargemethod, shortdescription,
 *     defraunitprice, fpsyear
 *   - Nullability per DDL constraints (NOT NULL vs nullable)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Verify owner CHECK constraint values (PT/PA/SD/LT) are
 *     enforced at service/validation layer — not duplicated in entity.
 *   - TRANSFORMENGINE TODO: PostgreSQL PARTITION BY LIST (fpsyear) — EF Core mapping
 *     in DataAccess layer must target the parent partitioned table (fps.testorproduct),
 *     not a partition slice directly.
 */

namespace Apha.FPS.Core.Entities
{
    /// <summary>
    /// Entity representing a test or product master record.
    /// Maps to fps.testorproduct (composite PK: ItemCode + FpsYear).
    /// The table is partitioned by fpsyear in PostgreSQL; EF maps to the parent table.
    /// </summary>
    public partial class TestOrProduct
    {
        // TRANSFORMENGINE: Composite PK — (ItemCode, FpsYear) per fps.testorproduct pk_testorproduct constraint
        public string ItemCode { get; set; } = null!;

        public int FpsYear { get; set; }

        // TRANSFORMENGINE: itemdescription character varying(200) — nullable per DDL
        public string? ItemDescription { get; set; }

        // TRANSFORMENGINE: testmanager character varying(50) — nullable per DDL
        public string? TestManager { get; set; }

        // TRANSFORMENGINE: jobstatus character varying(2) — nullable per DDL
        public string? JobStatus { get; set; }

        // TRANSFORMENGINE: unitpricevla money DEFAULT 0 — nullable money → decimal?
        public decimal? UnitPriceVla { get; set; }

        // TRANSFORMENGINE: priceahvg money — nullable money → decimal?
        public decimal? PriceAhvg { get; set; }

        // TRANSFORMENGINE: owner character varying(2) — nullable in DDL;
        //   CHECK constraint (owner IN ('PT','PA','SD','LT')) enforced at service layer
        public string? Owner { get; set; }

        // TRANSFORMENGINE: chargemethod character varying(5) — nullable per DDL
        public string? ChargeMethod { get; set; }

        // TRANSFORMENGINE: shortdescription character(18) — fixed-length char, mapped to string?
        public string? ShortDescription { get; set; }

        // TRANSFORMENGINE: defraunitprice money NOT NULL DEFAULT 0 — non-nullable decimal
        public decimal DefraUnitPrice { get; set; }
    }
}
