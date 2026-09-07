using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class FpsSettingStagingMap : IEntityTypeConfiguration<FpsSettingStaging>
    {
        public void Configure(EntityTypeBuilder<FpsSettingStaging> entity)
        {
            entity.HasKey(e => new { e.JobQueueId, e.Id }).HasName("pk_tblsettings_staging");

            entity.ToTable("tblsettings_staging", "fps");

            entity.Property(e => e.JobQueueId).HasColumnName("jobqueueid");

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .HasColumnName("id");

            entity.Property(e => e.Setting)
                .HasMaxLength(255)
                .HasColumnName("setting");

            entity.Property(e => e.Notes)
                .HasMaxLength(255)
                .HasColumnName("notes");

            entity.HasOne<BatchJobQueue>()
                .WithMany()
                .HasForeignKey(e => e.JobQueueId)
                .HasConstraintName("fk_tblsettings_staging_jobqueue");
        }
    }
}
