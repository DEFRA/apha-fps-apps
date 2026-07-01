/*
 * TRANSFORMENGINE MIGRATION — TestRequirementRCCostMap.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - New IEntityTypeConfiguration<TestRequirementRCCost> created from fps.tbltestrequirementrccost PostgreSQL DDL
 *   - Composite PK (testcode, buyer, profitcentre, fpsyear) registered via HasKey
 *     with pk_tbltestrequirementrccost constraint name
 *   - All column names lowercase per HasColumnName policy
 *   - ToTable targets fps.tbltestrequirementrccost (parent partitioned table)
 *   - price money NOT NULL (no DEFAULT) uses HasColumnType("money"), no HasDefaultValueSql
 *
 * PRESERVED:
 *   - All column names from DDL: testcode, buyer, profitcentre, price, fpsyear
 *   - Nullability constraints per DDL (price is NOT NULL with no DEFAULT)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: FK fk_tbltestrequirementrccost_testcode_buyer — validate
 *     TestCode + Buyer + FpsYear exists in fps.tlkptestreqmt at service layer before insert/update.
 *   - TRANSFORMENGINE TODO: FK fk_tbltestrequirementrccost_testcode_profitcentre — validate
 *     TestCode + ProfitCentre + FpsYear exists in fps.tbltestrccost at service layer before insert/update.
 *   - TRANSFORMENGINE TODO: PostgreSQL PARTITION BY LIST (fpsyear) — EF targets the parent table;
 *     verify partition routing is handled by the PostgreSQL server automatically.
 */

using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class TestRequirementRCCostMap : IEntityTypeConfiguration<TestRequirementRCCost>
    {
        public void Configure(EntityTypeBuilder<TestRequirementRCCost> entity)
        {
            // TRANSFORMENGINE: Composite PK (testcode, buyer, profitcentre, fpsyear)
            //   per pk_tbltestrequirementrccost constraint
            entity.HasKey(e => new { e.TestCode, e.Buyer, e.ProfitCentre, e.FpsYear })
                  .HasName("pk_tbltestrequirementrccost");

            // TRANSFORMENGINE: ToTable targets fps.tbltestrequirementrccost parent partitioned table
            entity.ToTable("tbltestrequirementrccost", "fps");

            entity.Property(e => e.TestCode)
                  .HasMaxLength(20)
                  .HasColumnName("testcode");

            // TRANSFORMENGINE: buyer character varying(20) NOT NULL — FK to fps.tlkptestreqmt
            entity.Property(e => e.Buyer)
                  .HasMaxLength(20)
                  .HasColumnName("buyer");

            // TRANSFORMENGINE: profitcentre character varying(50) NOT NULL — FK to fps.tbltestrccost
            entity.Property(e => e.ProfitCentre)
                  .HasMaxLength(50)
                  .HasColumnName("profitcentre");

            entity.Property(e => e.FpsYear)
                  .HasColumnName("fpsyear");

            // TRANSFORMENGINE: price money NOT NULL (no DEFAULT) — non-nullable money column, no default SQL
            entity.Property(e => e.Price)
                  .HasColumnType("money")
                  .HasColumnName("price");
        }
    }
}
