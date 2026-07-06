/*
 * TRANSFORMENGINE MIGRATION — ProgramManagerLinkMap.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository (Steps 7-7a)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New IEntityTypeConfiguration<ProgramManagerLink> map for mabarchive.tblprogram_manager_link
 *   - Composite PK (program, manager) — both varchar(50) string columns
 *   - All columns mapped with lowercase HasColumnName() as per project convention
 *
 * PRESERVED:
 *   - All column names and constraints from PostgreSQL DDL (mabarchive.tblprogram_manager_link)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
 */
using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PIMS.DataAccess.Data
{
    // TRANSFORMENGINE: maps mabarchive.tblprogram_manager_link; composite PK (program, manager)
    public class ProgramManagerLinkMap : IEntityTypeConfiguration<ProgramManagerLink>
    {
        public void Configure(EntityTypeBuilder<ProgramManagerLink> entity)
        {
            // TRANSFORMENGINE: composite string PK as per DDL CONSTRAINT pk_tblprogram_manager_link PRIMARY KEY (program, manager)
            entity.HasKey(e => new { e.Program, e.Manager }).HasName("pk_tblprogram_manager_link");

            entity.ToTable("tblprogram_manager_link", "mabarchive");

            entity.Property(e => e.Program)
                .HasMaxLength(50)
                .HasColumnName("program");

            entity.Property(e => e.Manager)
                .HasMaxLength(50)
                .HasColumnName("manager");
        }
    }
}
