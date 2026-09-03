using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class YearEndMonthHourStagingMap : IEntityTypeConfiguration<YearEndMonthHourStaging>
    {
        public void Configure(EntityTypeBuilder<YearEndMonthHourStaging> entity)
        {
            entity.HasKey(e => new { e.JobQueueId, e.Month, e.Fmonth }).HasName("pk_yearend_monthhours_staging");

            entity.ToTable("yearend_monthhours_staging", "fps");

            entity.Property(e => e.JobQueueId).HasColumnName("jobqueueid");
            entity.Property(e => e.MonthYear).HasColumnName("month_year");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.Fmonth).HasColumnName("fmonth");

            entity.Property(e => e.Days)
                .HasPrecision(5, 1)
                .HasColumnName("days");
            entity.Property(e => e.CvlHours)
                .HasPrecision(5, 1)
                .HasColumnName("cvlhours");
            entity.Property(e => e.VidHours)
                .HasPrecision(5, 1)
                .HasColumnName("vidhours");

            entity.HasOne<BatchJobQueue>()
                .WithMany()
                .HasForeignKey(e => e.JobQueueId)
                .HasConstraintName("fk_yearend_monthhours_staging_jobqueue");
        }
    }
}
