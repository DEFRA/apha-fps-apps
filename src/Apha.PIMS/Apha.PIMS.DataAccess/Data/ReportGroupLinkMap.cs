/*
 * TRANSFORMENGINE MIGRATION — ReportGroupLinkMap.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository (Steps 7-7a)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New IEntityTypeConfiguration<ReportGroupLink> map for mabarchive.tblreportgroup_link
 *   - Composite PK (reportid, groupid) mapped via HasKey anonymous type
 *   - FK to tblreportgroup(groupid) defined per DDL
 *   - All columns mapped with lowercase HasColumnName() as per project convention
 *
 * PRESERVED:
 *   - All column names and constraints from PostgreSQL DDL (mabarchive.tblreportgroup_link)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
 */
using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PIMS.DataAccess.Data
{
    // TRANSFORMENGINE: maps mabarchive.tblreportgroup_link; composite PK (reportid, groupid)
    public class ReportGroupLinkMap : IEntityTypeConfiguration<ReportGroupLink>
    {
        public void Configure(EntityTypeBuilder<ReportGroupLink> entity)
        {
            // TRANSFORMENGINE: composite PK as per DDL CONSTRAINT pk_tblreportgroup_link PRIMARY KEY (reportid, groupid)
            entity.HasKey(e => new { e.Reportid, e.Groupid }).HasName("pk_tblreportgroup_link");

            entity.ToTable("tblreportgroup_link", "mabarchive");

            entity.Property(e => e.Reportid)
                .HasColumnName("reportid");

            entity.Property(e => e.Groupid)
                .HasColumnName("groupid");
        }
    }
}
