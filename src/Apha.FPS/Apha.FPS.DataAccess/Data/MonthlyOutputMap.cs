using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class MonthlyOutputMap : IEntityTypeConfiguration<MonthlyOutput>
    {
        public void Configure(EntityTypeBuilder<MonthlyOutput> builder)
        {
            builder.ToTable("monthlyoutput", "fps");
            builder.HasKey(x => new { x.TestCode, x.Buyer, x.Month, x.WorkGroup });
            builder.Property(x => x.TestCode).HasColumnName("testcode");
            builder.Property(x => x.Buyer).HasColumnName("buyer");
            builder.Property(x => x.Month).HasColumnName("month");
            builder.Property(x => x.WorkGroup).HasColumnName("workgroup");
            builder.Property(x => x.Volume).HasColumnName("volume");
            builder.Property(x => x.WgBuyer).HasColumnName("wgbuyer");
            builder.Property(x => x.FpsYear).HasColumnName("fpsyear");
        }
    }
}
