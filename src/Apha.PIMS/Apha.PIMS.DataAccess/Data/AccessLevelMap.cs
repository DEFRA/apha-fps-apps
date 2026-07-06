/*
 * TRANSFORMENGINE MIGRATION — AccessLevelMap.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository (Steps 7-7a)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New IEntityTypeConfiguration<AccessLevel> map for mabarchive.tblaccesslevels
 *   - Composite PK (systemid, accesslevelid) as per DDL
 *   - FK: systemid references mabarchive.tblaccesssystems(systemid)
 *   - All columns mapped with lowercase HasColumnName() as per project convention
 *
 * PRESERVED:
 *   - All column names and constraints from PostgreSQL DDL (mabarchive.tblaccesslevels)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
 */
using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PIMS.DataAccess.Data
{
    // TRANSFORMENGINE: maps mabarchive.tblaccesslevels; composite PK (systemid, accesslevelid)
    public class AccessLevelMap : IEntityTypeConfiguration<AccessLevel>
    {
        public void Configure(EntityTypeBuilder<AccessLevel> entity)
        {
            // TRANSFORMENGINE: composite PK as per DDL CONSTRAINT pk_tblaccesslevels PRIMARY KEY (systemid, accesslevelid)
            entity.HasKey(e => new { e.Systemid, e.Accesslevelid }).HasName("pk_tblaccesslevels");

            entity.ToTable("tblaccesslevels", "mabarchive");

            entity.Property(e => e.Systemid)
                .HasColumnName("systemid");

            entity.Property(e => e.Accesslevelid)
                .HasColumnName("accesslevelid");

            entity.Property(e => e.Accesslevel)
                .HasMaxLength(50)
                .HasColumnName("accesslevel");
        }
    }
}
