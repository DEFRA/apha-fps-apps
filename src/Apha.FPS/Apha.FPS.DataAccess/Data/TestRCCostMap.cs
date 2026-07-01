/*
 * TRANSFORMENGINE MIGRATION — TestRCCostMap.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - New IEntityTypeConfiguration<TestRCCost> created from fps.tbltestrccost PostgreSQL DDL
 *   - Composite PK (testcode, profitcentre, fpsyear) registered via HasKey with pk_tbltestrccost
 *   - All column names lowercase per HasColumnName policy
 *   - ToTable targets fps.tbltestrccost (parent partitioned table)
 *   - price money NOT NULL DEFAULT 0 uses HasColumnType("money") with HasDefaultValueSql("0")
 *
 * PRESERVED:
 *   - All column names from DDL: testcode, profitcentre, price, fpsyear
 *   - Nullability constraints per DDL (price is NOT NULL DEFAULT 0)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: FK fk_tbltestrccost_testcode — validate TestCode + FpsYear
 *     exists in fps.testorproduct at service layer before insert/update.
 *   - TRANSFORMENGINE TODO: FK fk_tbltestrccost_profitcentre — validate ProfitCentre
 *     exists in fps.tblkpprofitcentre at service layer before insert/update.
 *   - TRANSFORMENGINE TODO: PostgreSQL PARTITION BY LIST (fpsyear) — EF targets the parent table;
 *     verify partition routing is handled by the PostgreSQL server automatically.
 */

using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class TestRCCostMap : IEntityTypeConfiguration<TestRCCost>
    {
        public void Configure(EntityTypeBuilder<TestRCCost> entity)
        {
            // TRANSFORMENGINE: Composite PK (testcode, profitcentre, fpsyear) per pk_tbltestrccost constraint
            entity.HasKey(e => new { e.TestCode, e.ProfitCentre, e.FpsYear })
                  .HasName("pk_tbltestrccost");

            // TRANSFORMENGINE: ToTable targets fps.tbltestrccost parent partitioned table
            entity.ToTable("tbltestrccost", "fps");

            entity.Property(e => e.TestCode)
                  .HasMaxLength(20)
                  .HasColumnName("testcode");

            // TRANSFORMENGINE: profitcentre character varying(50) NOT NULL — FK to fps.tblkpprofitcentre
            entity.Property(e => e.ProfitCentre)
                  .HasMaxLength(50)
                  .HasColumnName("profitcentre");

            entity.Property(e => e.FpsYear)
                  .HasColumnName("fpsyear");

            // TRANSFORMENGINE: price money NOT NULL DEFAULT 0 — non-nullable money column
            entity.Property(e => e.Price)
                  .HasDefaultValueSql("0")
                  .HasColumnType("money")
                  .HasColumnName("price");
        }
    }
}
