using Apha.PACT.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PACT.DataAccess.Data
{
    public class ReleasePeriodMap : IEntityTypeConfiguration<ReleasePeriod>
    {
        public void Configure(EntityTypeBuilder<ReleasePeriod> entity)
        {
            entity.HasKey(e => new { e.PeriodName, e.FpsYear }).HasName("pk_tblperiod");

            entity.ToTable("tblperiod", "fps");

            entity.HasIndex(e => e.EndPeriod, "endperiod");

            entity.Property(e => e.PeriodName)
                .HasMaxLength(50)
                .HasColumnName("periodname");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.EndPeriod).HasColumnName("endperiod");
            entity.Property(e => e.FinalSummariesRun).HasColumnName("finalsummariesrun");
            entity.Property(e => e.PeriodLocked)
            .HasDefaultValue((short)0)
            .HasColumnName("periodlocked");
            entity.Property(e => e.PeriodType)
                .HasMaxLength(50)
                .HasColumnName("periodtype");
            entity.Property(e => e.StartPeriod).HasColumnName("startperiod");
        }
    }
}