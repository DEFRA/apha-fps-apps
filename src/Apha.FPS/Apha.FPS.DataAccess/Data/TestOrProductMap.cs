/*
 * TRANSFORMENGINE MIGRATION — TestOrProductMap.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - New IEntityTypeConfiguration<TestOrProduct> created from fps.testorproduct PostgreSQL DDL
 *   - Composite PK (itemcode, fpsyear) registered via HasKey with constraint name pk_testorproduct
 *   - All column names lowercase per HasColumnName policy
 *   - ToTable targets fps.testorproduct (parent partitioned table)
 *   - money columns (unitpricevla, priceahvg, defraunitprice) use HasColumnType("money")
 *   - owner CHECK constraint preserved as comment; not enforced in EF map layer
 *
 * PRESERVED:
 *   - All column names from DDL: itemcode, itemdescription, testmanager, jobstatus,
 *     unitpricevla, priceahvg, owner, chargemethod, shortdescription, defraunitprice, fpsyear
 *   - Nullability constraints per DDL
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: owner CHECK constraint (PT/PA/SD/LT) must be enforced at service layer.
 *   - TRANSFORMENGINE TODO: PostgreSQL PARTITION BY LIST (fpsyear) — EF targets the parent table;
 *     verify partition routing is handled by the PostgreSQL server automatically.
 */

using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class TestOrProductMap : IEntityTypeConfiguration<TestOrProduct>
    {
        public void Configure(EntityTypeBuilder<TestOrProduct> entity)
        {
            // TRANSFORMENGINE: Composite PK (itemcode, fpsyear) per pk_testorproduct constraint
            entity.HasKey(e => new { e.ItemCode, e.FpsYear })
                  .HasName("pk_testorproduct");

            // TRANSFORMENGINE: ToTable targets fps.testorproduct parent partitioned table
            entity.ToTable("testorproduct", "fps");

            entity.Property(e => e.ItemCode)
                  .HasMaxLength(20)
                  .HasColumnName("itemcode");

            entity.Property(e => e.FpsYear)
                  .HasColumnName("fpsyear");

            entity.Property(e => e.ItemDescription)
                  .HasMaxLength(200)
                  .HasColumnName("itemdescription");

            entity.Property(e => e.TestManager)
                  .HasMaxLength(50)
                  .HasColumnName("testmanager");

            entity.Property(e => e.JobStatus)
                  .HasMaxLength(2)
                  .HasColumnName("jobstatus");

            // TRANSFORMENGINE: unitpricevla money DEFAULT 0 — nullable money column
            entity.Property(e => e.UnitPriceVla)
                  .HasDefaultValueSql("0")
                  .HasColumnType("money")
                  .HasColumnName("unitpricevla");

            // TRANSFORMENGINE: priceahvg money — nullable, no default
            entity.Property(e => e.PriceAhvg)
                  .HasColumnType("money")
                  .HasColumnName("priceahvg");

            // TRANSFORMENGINE: owner character varying(2) — nullable;
            //   CHECK constraint (owner IN ('PT','PA','SD','LT')) enforced at service layer
            entity.Property(e => e.Owner)
                  .HasMaxLength(2)
                  .HasColumnName("owner");

            entity.Property(e => e.ChargeMethod)
                  .HasMaxLength(5)
                  .HasColumnName("chargemethod");

            // TRANSFORMENGINE: shortdescription character(18) — fixed-length char type
            entity.Property(e => e.ShortDescription)
                  .HasMaxLength(18)
                  .IsFixedLength()
                  .HasColumnName("shortdescription");

            // TRANSFORMENGINE: defraunitprice money NOT NULL DEFAULT 0 — non-nullable
            entity.Property(e => e.DefraUnitPrice)
                  .HasDefaultValueSql("0")
                  .HasColumnType("money")
                  .HasColumnName("defraunitprice");
        }
    }
}
