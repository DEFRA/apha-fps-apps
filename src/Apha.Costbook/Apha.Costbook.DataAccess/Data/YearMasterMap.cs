using Apha.Costbook.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.Costbook.DataAccess.Data
{
    public class YearMasterMap : IEntityTypeConfiguration<YearMaster>
    {
        public void Configure(EntityTypeBuilder<YearMaster> entity)
        {
            entity.HasKey(e => e.FpsYear).HasName("pk_tblyearmaster");

            entity.ToTable("tblyearmaster", DbConstants.FpsSchemaName, tb => tb.HasComment("Master table of fiscal / FPS years. Defines which years exist, their display codes, and their lifecycle status (Open, Closed, Planned)."));

            entity.HasIndex(e => e.FpsYearCode, "uq_tblyearmaster_fpsyearcode").IsUnique();

            entity.Property(e => e.FpsYear)
                .ValueGeneratedNever()
                .HasComment("Four-digit calendar year that starts the fiscal period (e.g. 2025 for Apr 2025 – Mar 2026). Primary key.")
                .HasColumnName("fpsyear");
            entity.Property(e => e.Active)
                .HasDefaultValue(true)
                .HasComment("Soft-delete flag. TRUE = visible to the application.")
                .HasColumnName("active");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .HasComment("User or service account that created the row.")
                .HasColumnName("createdby");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("now()")
                .HasComment("Timestamp when the row was first inserted (auto-set).")
                .HasColumnName("createdon");
            entity.Property(e => e.FpsYearCode)
                .HasMaxLength(20)
                .HasComment("Human-readable fiscal-year label, e.g. FPS2025-26.")
                .HasColumnName("fpsyearcode");
            entity.Property(e => e.Remarks)
                .HasComment("Free-text note explaining the status or special conditions.")
                .HasColumnName("remarks");
            entity.Property(e => e.YearStatus)
                .HasMaxLength(10)
                .HasComment("Lifecycle state: Open (transactions allowed), Closed (read-only), or Planned (configuration only).")
                .HasColumnName("yearstatus");
        }
    }
}
