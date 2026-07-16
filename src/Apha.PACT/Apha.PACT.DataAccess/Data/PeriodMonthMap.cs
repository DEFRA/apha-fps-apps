using Apha.PACT.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PACT.DataAccess.Data
{
    public class PeriodMonthMap : IEntityTypeConfiguration<PeriodMonth>
    {
        public void Configure(EntityTypeBuilder<PeriodMonth> entity)
        {
            entity
                .HasNoKey()
                .ToView("tblkperiodmonth", "fps");

            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.EndMonth).HasColumnName("endmonth");
            entity.Property(e => e.MonthNo).HasColumnName("monthno");
            entity.Property(e => e.PeriodName)
                .HasMaxLength(50)
                .HasColumnName("periodname");
        }
    }
}