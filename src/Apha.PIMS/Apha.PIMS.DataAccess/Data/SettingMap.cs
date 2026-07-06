/*
 * TRANSFORMENGINE MIGRATION — SettingMap.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository (Steps 7-7a)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New IEntityTypeConfiguration<Setting> map for mabarchive.tbl_settings
 *   - PK id is varchar(50) string — ValueGeneratedNever()
 *   - DDL column 'setting' maps to entity property SettingValue (aliased to avoid C# keyword clash)
 *   - All columns mapped with lowercase HasColumnName() as per project convention
 *
 * PRESERVED:
 *   - All column names and constraints from PostgreSQL DDL (mabarchive.tbl_settings)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
 */
using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PIMS.DataAccess.Data
{
    // TRANSFORMENGINE: maps mabarchive.tbl_settings; string PK id; DDL 'setting' column → SettingValue property
    public class SettingMap : IEntityTypeConfiguration<Setting>
    {
        public void Configure(EntityTypeBuilder<Setting> entity)
        {
            entity.HasKey(e => e.Id).HasName("pk_tbl_settings");

            entity.ToTable("tbl_settings", "mabarchive");

            // TRANSFORMENGINE: varchar(50) string PK — not generated
            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .ValueGeneratedNever()
                .HasColumnName("id");

            // TRANSFORMENGINE: DDL column name 'setting' — entity property SettingValue
            entity.Property(e => e.SettingValue)
                .HasMaxLength(255)
                .HasColumnName("setting");

            entity.Property(e => e.Notes)
                .HasMaxLength(255)
                .HasColumnName("notes");

            entity.Property(e => e.Testsetting)
                .HasMaxLength(255)
                .HasColumnName("testsetting");

            entity.Property(e => e.Userupdateable)
                .HasDefaultValue(false)
                .HasColumnName("userupdateable");
        }
    }
}
