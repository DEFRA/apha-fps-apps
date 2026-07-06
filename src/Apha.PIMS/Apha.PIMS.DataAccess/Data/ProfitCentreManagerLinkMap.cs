/*
 * TRANSFORMENGINE MIGRATION — ProfitCentreManagerLinkMap.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository (Steps 7-7a)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New IEntityTypeConfiguration<ProfitCentreManagerLink> map for mabarchive.tblprofitcentre_manager_link
 *   - Composite PK (profitcentre, manager) — both varchar(50) string columns
 *   - All columns mapped with lowercase HasColumnName() as per project convention
 *
 * PRESERVED:
 *   - All column names and constraints from PostgreSQL DDL (mabarchive.tblprofitcentre_manager_link)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
 */
using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PIMS.DataAccess.Data
{
    // TRANSFORMENGINE: maps mabarchive.tblprofitcentre_manager_link; composite PK (profitcentre, manager)
    public class ProfitCentreManagerLinkMap : IEntityTypeConfiguration<ProfitCentreManagerLink>
    {
        public void Configure(EntityTypeBuilder<ProfitCentreManagerLink> entity)
        {
            // TRANSFORMENGINE: composite string PK as per DDL CONSTRAINT pk_tblprofitcentre_manager_link PRIMARY KEY (profitcentre, manager)
            entity.HasKey(e => new { e.Profitcentre, e.Manager }).HasName("pk_tblprofitcentre_manager_link");

            entity.ToTable("tblprofitcentre_manager_link", "mabarchive");

            entity.Property(e => e.Profitcentre)
                .HasMaxLength(50)
                .HasColumnName("profitcentre");

            entity.Property(e => e.Manager)
                .HasMaxLength(50)
                .HasColumnName("manager");
        }
    }
}
