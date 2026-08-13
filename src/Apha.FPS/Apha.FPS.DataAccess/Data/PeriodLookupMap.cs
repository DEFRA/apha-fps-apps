using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class PeriodLookupMap : IEntityTypeConfiguration<PeriodLookup>
    {
        public void Configure(EntityTypeBuilder<PeriodLookup> builder)
        {
            builder.HasNoKey();
            builder.ToView("tblkperiodmonth", "fps");

            builder.Property(e => e.FpsYear)
                .HasColumnName("fpsyear");

            builder.Property(e => e.AccntsPeriod)
                .HasColumnName("endmonth");

            builder.Property(e => e.MonthName)
                .HasMaxLength(50)
                .HasColumnName("periodname");

            builder.Property(e => e.MonthNumber)
                .HasColumnName("monthno");
        }
    }
}
