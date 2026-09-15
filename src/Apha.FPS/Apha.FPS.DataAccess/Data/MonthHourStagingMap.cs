using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class MonthHourStagingMap : IEntityTypeConfiguration<MonthHourStaging>
    {
        public void Configure(EntityTypeBuilder<MonthHourStaging> entity)
        {
            entity.HasKey(e => new { e.Year, e.Month, e.FpsYear }).HasName("pk_tlkpmonthhours_staging");

            entity.ToTable("tlkpmonthhours_staging", "fps");

            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.CvlHours)
                .HasPrecision(5, 1)
                .HasColumnName("cvlhours");
            entity.Property(e => e.Days)
                .HasPrecision(5, 1)
                .HasColumnName("days");
            entity.Property(e => e.Fmonth).HasColumnName("fmonth");
            entity.Property(e => e.VidHours)
                .HasPrecision(5, 1)
                .HasColumnName("vidhours");
        }
    }
}
