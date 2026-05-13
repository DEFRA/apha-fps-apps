using Apha.PACT.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PACT.DataAccess.Data
{
    public class MonthMap : IEntityTypeConfiguration<Month>
    {
        public void Configure(EntityTypeBuilder<Month> builder)
        {
            builder.ToTable("tblkpmonth", "fps");
            builder.HasKey(e => e.MonthNumber);

            builder.Property(e => e.MonthNumber)
                .HasColumnName("monthnumber");

            builder.Property(e => e.MonthName)
                .HasMaxLength(20)
                .HasColumnName("monthname");
        }
    }
}