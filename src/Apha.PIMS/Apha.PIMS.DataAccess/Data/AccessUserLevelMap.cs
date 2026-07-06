/*
 * TRANSFORMENGINE MIGRATION — AccessUserLevelMap.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository (Steps 7-7a)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New IEntityTypeConfiguration<AccessUserLevel> map for mabarchive.tblaccessusers_levels
 *   - Three-column composite PK (systemid, ntlogin, accesslevelid) as per DDL
 *   - FK to tblaccesslevels(systemid, accesslevelid) and tblaccessusers(systemid, ntlogin)
 *   - All columns mapped with lowercase HasColumnName() as per project convention
 *
 * PRESERVED:
 *   - All column names and constraints from PostgreSQL DDL (mabarchive.tblaccessusers_levels)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
 */
using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PIMS.DataAccess.Data
{
    // TRANSFORMENGINE: maps mabarchive.tblaccessusers_levels; three-column composite PK (systemid, ntlogin, accesslevelid)
    public class AccessUserLevelMap : IEntityTypeConfiguration<AccessUserLevel>
    {
        public void Configure(EntityTypeBuilder<AccessUserLevel> entity)
        {
            // TRANSFORMENGINE: three-column composite PK as per DDL CONSTRAINT pk_tblaccessusers_levels PRIMARY KEY (systemid, ntlogin, accesslevelid)
            entity.HasKey(e => new { e.Systemid, e.Ntlogin, e.Accesslevelid }).HasName("pk_tblaccessusers_levels");

            entity.ToTable("tblaccessusers_levels", "mabarchive");

            entity.Property(e => e.Systemid)
                .HasColumnName("systemid");

            entity.Property(e => e.Ntlogin)
                .HasMaxLength(50)
                .HasColumnName("ntlogin");

            entity.Property(e => e.Accesslevelid)
                .HasColumnName("accesslevelid");
        }
    }
}
