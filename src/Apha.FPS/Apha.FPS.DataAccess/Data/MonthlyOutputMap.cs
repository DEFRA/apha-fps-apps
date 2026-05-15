using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class MonthlyOutputMap : IEntityTypeConfiguration<MonthlyOutput>
    {
        public void Configure(EntityTypeBuilder<MonthlyOutput> builder)
        {
            builder.HasKey(x => new { x.TestCode, x.Buyer, x.Month, x.WorkGroup, x.FpsYear })
                   .HasName("pk_monthlyoutput");

            builder.ToTable("monthlyoutput", "fps");

            builder.HasIndex(x => x.Month, "month");
            builder.HasIndex(x => x.WorkGroup, "monthlyoutput_workgroup");
            builder.HasIndex(x => new { x.TestCode, x.Buyer }, "reference14");
            builder.HasIndex(x => new { x.WorkGroup, x.TestCode }, "reference25");
            builder.HasIndex(x => x.TestCode, "testcode");

            builder.Property(x => x.TestCode)
                .HasMaxLength(20)
                .HasColumnName("testcode");
            builder.Property(x => x.Buyer)
                .HasMaxLength(20)
                .HasColumnName("buyer");
            builder.Property(x => x.Month).HasColumnName("month");
            builder.Property(x => x.WorkGroup)
                .HasMaxLength(50)
                .HasColumnName("workgroup");
            builder.Property(x => x.FpsYear).HasColumnName("fpsyear");
            builder.Property(x => x.Volume).HasColumnName("volume");
            builder.Property(x => x.WgBuyer)
                .HasMaxLength(50)
                .HasColumnName("wgbuyer");
        }
    }
}
