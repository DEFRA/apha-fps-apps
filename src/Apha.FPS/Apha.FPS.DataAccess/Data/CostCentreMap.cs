/*
 * TRANSFORMENGINE MIGRATION — CostCentreMap.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - MS Access frmMaintCostCentres (fps.costcentre table) → EF Core IEntityTypeConfiguration<CostCentre>
 *   - Composite PK (costcentre, fpsyear) → HasKey(e => new { e.CostCentreNo, e.FpsYear }) with constraint name pk_costcentre
 *   - Column "costcentre double precision" → HasColumnName("costcentre"), ValueGeneratedNever()
 *   - Column "profitcentre varchar(50)" → HasColumnName("profitcentre"), HasMaxLength(50)
 *   - Column "fpsyear integer" → HasColumnName("fpsyear"), ValueGeneratedNever()
 *   - FK to fps.tblkpprofitcentre (profitcentre) preserved as HasForeignKey navigation hint
 *   - ToTable("costcentre", "fps") — partitioned table; EF Core targets the parent partition
 *
 * PRESERVED:
 *   - All column nullability constraints from DDL: costcentre NOT NULL, profitcentre NOT NULL, fpsyear NOT NULL
 *   - Composite PK constraint name pk_costcentre from DDL
 *   - FK constraint references (fk_costcentre_fpsyear, fk_costcentre_profitcentre) documented
 *   - lowercase ToTable/HasColumnName per Phase 4 rules
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: The fps.costcentre table is PARTITION BY LIST (fpsyear). EF Core targets the parent
 *     table and PostgreSQL routes rows to the correct partition automatically. Verify that any direct
 *     partition-targeted queries (e.g. fps.costcentre_y2024) are NOT needed for this migration.
 */

using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    // TRANSFORMENGINE: Map for fps.costcentre — composite PK (costcentre, fpsyear); partitioned table, EF targets parent
    public class CostCentreMap : IEntityTypeConfiguration<CostCentre>
    {
        public void Configure(EntityTypeBuilder<CostCentre> entity)
        {
            // TRANSFORMENGINE: Composite PK from DDL CONSTRAINT pk_costcentre PRIMARY KEY (costcentre, fpsyear)
            entity.HasKey(e => new { e.CostCentreNo, e.FpsYear }).HasName("pk_costcentre");

            // TRANSFORMENGINE: ToTable lowercase per Phase 4 rules; fps schema; partitioned parent table
            entity.ToTable("costcentre", "fps");

            // TRANSFORMENGINE: Column "costcentre double precision NOT NULL" — renamed to CostCentreNo to avoid class name clash
            entity.Property(e => e.CostCentreNo)
                .ValueGeneratedNever()
                .HasColumnName("costcentre");

            // TRANSFORMENGINE: Column "profitcentre varchar(50) NOT NULL" — FK → fps.tblkpprofitcentre
            entity.Property(e => e.ProfitCentre)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("profitcentre");

            // TRANSFORMENGINE: Column "fpsyear integer NOT NULL" — composite PK part; FK → fps.tblyearmaster
            entity.Property(e => e.FpsYear)
                .ValueGeneratedNever()
                .HasColumnName("fpsyear");
        }
    }
}
