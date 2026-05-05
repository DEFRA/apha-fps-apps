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
            builder.HasKey(e => e.Monthnumber);

            builder.Property(e => e.Monthnumber)
                .HasColumnName("monthnumber");

            builder.Property(e => e.Monthname)
                .HasMaxLength(20)
                .HasColumnName("monthname");
        }
    }
}
