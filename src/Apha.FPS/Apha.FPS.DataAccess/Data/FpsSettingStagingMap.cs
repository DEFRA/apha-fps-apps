using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class FpsSettingStagingMap : IEntityTypeConfiguration<FpsSettingStaging>
    {
        public void Configure(EntityTypeBuilder<FpsSettingStaging> entity)
        {
            entity.HasKey(e => new { e.Id, e.FpsYear }).HasName("pk_tblsettings_staging");

            entity.ToTable("tblsettings_staging", "fps");

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .HasColumnName("id");

            entity.Property(e => e.Setting)
                .HasMaxLength(255)
                .HasColumnName("setting");

            entity.Property(e => e.Notes)
                .HasMaxLength(255)
                .HasColumnName("notes");

            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");

            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100)
                .HasColumnName("updated_by");

            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        }
    }
}
