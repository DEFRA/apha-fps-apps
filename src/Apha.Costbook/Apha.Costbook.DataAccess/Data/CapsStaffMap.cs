/*
 * TRANSFORMENGINE MIGRATION — CapsStaffMap.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New IEntityTypeConfiguration<CapsStaff> map created for mabarchive.tblcapsstaff
 *   - MNumber mapped as string PK (mnumber, varchar 50)
 *   - Name mapped as required string (name, varchar 50, NOT NULL)
 *   - Dt2Number mapped as nullable string (dt2number, varchar 50)
 *   - ToTable uses DbConstants.MabArchiveSchemaName ("mabarchive") per project convention
 *
 * PRESERVED:
 *   - All HasColumnName values lowercase per project convention
 *   - Schema name sourced from DbConstants.MabArchiveSchemaName
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm exact PK constraint name matches PostgreSQL DDL (currently inferred as pk_tblcapsstaff)
 */

using Apha.Costbook.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.Costbook.DataAccess.Data
{
    // TRANSFORMENGINE: EF Core map for mabarchive.tblcapsstaff — MNumber string PK, Name NOT NULL, Dt2Number nullable
    public class CapsStaffMap : IEntityTypeConfiguration<CapsStaff>
    {
        public void Configure(EntityTypeBuilder<CapsStaff> entity)
        {
            entity.HasKey(e => e.MNumber).HasName("pk_tblcapsstaff");

            entity.ToTable("tblcapsstaff", DbConstants.MabArchiveSchemaName);

            entity.Property(e => e.MNumber)
                .HasMaxLength(50)
                .HasColumnName("mnumber");

            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsRequired()
                .HasColumnName("name");

            entity.Property(e => e.Dt2Number)
                .HasMaxLength(50)
                .HasColumnName("dt2number");
        }
    }
}
