using Apha.PACT.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PACT.DataAccess.Data
{
    public class CalenderMonthMap : IEntityTypeConfiguration<CalenderMonth>
    {
        public void Configure(EntityTypeBuilder<CalenderMonth> entity)
        {
            entity
                 .HasNoKey()
                 .ToTable("tlkpmonth", "fps");

            entity.Property(e => e.AccntsPeriod).HasColumnName("accntsperiod");
            entity.Property(e => e.FQuarter).HasColumnName("fquarter");
            entity.Property(e => e.MonthName)
                .HasMaxLength(50)
                .HasColumnName("monthname");
            entity.Property(e => e.MonthNumber).HasColumnName("monthnumber");
        }
    }
}