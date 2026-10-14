using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class ArchiveMonthlyOutputMap : IEntityTypeConfiguration<ArchiveMonthlyOutput>
    {
        public void Configure(EntityTypeBuilder<ArchiveMonthlyOutput> builder)
        {
            builder.HasKey(e => new { e.Year, e.TestCode, e.Buyer, e.Month, e.WorkGroup }).HasName("pk_my_monthlyoutput");
            builder.ToTable("my_monthlyoutput", "mabarchive", t => t.ExcludeFromMigrations());
            builder.Property(e => e.Year).HasColumnName("year");
            builder.Property(e => e.TestCode).HasMaxLength(20).HasColumnName("testcode");
            builder.Property(e => e.Buyer).HasMaxLength(20).HasColumnName("buyer");
            builder.Property(e => e.Month).HasColumnName("month");
            builder.Property(e => e.WorkGroup).HasMaxLength(50).HasColumnName("workgroup");
            builder.Property(e => e.Volume).HasColumnName("volume");
            builder.Property(e => e.WgBuyer).HasMaxLength(50).HasColumnName("wgbuyer");
        }
    }
}
