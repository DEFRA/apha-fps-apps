using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class MonthlyOutputMap : IEntityTypeConfiguration<MonthlyOutput>
    {
        public void Configure(EntityTypeBuilder<MonthlyOutput> entity)
        {
            entity.HasKey(e => new { e.TestCode, e.Buyer, e.Month, e.WorkGroup, e.FpsYear })
                .HasName("pk_monthlyoutput");

            entity.ToTable("monthlyoutput", "fps");

            entity.Property(e => e.TestCode)
                .HasColumnType("citext")
                .HasColumnName("testcode");
            entity.Property(e => e.Buyer)
                .HasColumnType("citext")
                .HasColumnName("buyer");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.WorkGroup)
                .HasColumnType("citext")
                .HasColumnName("workgroup");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.Volume).HasColumnName("volume");
            entity.Property(e => e.WgBuyer)
                .HasMaxLength(50)
                .HasColumnName("wgbuyer");
        }
    }
}
