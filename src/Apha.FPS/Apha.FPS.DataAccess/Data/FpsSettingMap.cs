using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class FpsSettingMap : IEntityTypeConfiguration<FpsSetting>
    {


        public void Configure(EntityTypeBuilder<FpsSetting> entity)
        {
            entity.HasKey(e => new { e.Id, e.FpsYear }).HasName("pk_tblsettings");

            entity.ToTable("tblsettings", "fps", tb => tb.HasComment("Application-level configuration settings. Only business-logic constants belong here; infrastructure config moves to appsettings.json."));

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .HasComment("Unique setting key referenced by application code.")
                .HasColumnName("id");
            entity.Property(e => e.FpsYear)
                .HasComment("Fiscal year scope. NULL = not year-specific.")
                .HasColumnName("fpsyear");
            entity.Property(e => e.Notes)
                .HasMaxLength(255)
                .HasComment("Free-text description of purpose, origin, and usage.")
                .HasColumnName("notes");
            entity.Property(e => e.Setting)
                .HasMaxLength(255)
                .HasComment("The setting value as text.")
                .HasColumnName("setting");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasComment("Timestamp of last modification (auto-set on insert).")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100)
                .HasComment("User or service account that last modified the row.")
                .HasColumnName("updated_by");
        }
    }
}
