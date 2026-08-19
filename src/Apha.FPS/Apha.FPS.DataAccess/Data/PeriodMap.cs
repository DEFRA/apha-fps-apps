using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class PeriodMap : IEntityTypeConfiguration<Period>
    {
        public void Configure(EntityTypeBuilder<Period> builder)
        {
            builder.HasKey(e => new { e.PeriodName, e.FpsYear });
            builder.ToTable("tblperiod", "fps");

            builder.Property(e => e.PeriodName)
                .HasMaxLength(50)
                .HasColumnName("periodname");

            builder.Property(e => e.FpsYear)
                .HasColumnName("fpsyear");

            builder.Property(e => e.EndPeriod)
                .HasColumnName("endperiod");

            builder.Property(e => e.FinalSummariesRun)
                .HasColumnName("finalsummariesrun");

            builder.Property(e => e.PeriodLocked)
                .HasColumnName("periodlocked");
        }
    }
}
