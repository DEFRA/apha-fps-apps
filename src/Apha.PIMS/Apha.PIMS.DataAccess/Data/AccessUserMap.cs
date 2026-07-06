/*
 * TRANSFORMENGINE MIGRATION — AccessUserMap.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository (Steps 7-7a)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New IEntityTypeConfiguration<AccessUser> map for mabarchive.tblaccessusers
 *   - Composite PK (systemid, ntlogin) as per DDL
 *   - FK: systemid references mabarchive.tblaccesssystems(systemid)
 *   - All columns mapped with lowercase HasColumnName() as per project convention
 *
 * PRESERVED:
 *   - All column names and constraints from PostgreSQL DDL (mabarchive.tblaccessusers)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
 */
using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PIMS.DataAccess.Data
{
    // TRANSFORMENGINE: maps mabarchive.tblaccessusers; composite PK (systemid, ntlogin)
    public class AccessUserMap : IEntityTypeConfiguration<AccessUser>
    {
        public void Configure(EntityTypeBuilder<AccessUser> entity)
        {
            // TRANSFORMENGINE: composite PK as per DDL CONSTRAINT pk_tblaccessusers PRIMARY KEY (systemid, ntlogin)
            entity.HasKey(e => new { e.Systemid, e.Ntlogin }).HasName("pk_tblaccessusers");

            entity.ToTable("tblaccessusers", "mabarchive");

            entity.Property(e => e.Systemid)
                .HasColumnName("systemid");

            entity.Property(e => e.Ntlogin)
                .HasMaxLength(50)
                .HasColumnName("ntlogin");

            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");

            entity.Property(e => e.Dt2login)
                .HasMaxLength(50)
                .HasColumnName("dt2login");
        }
    }
}
