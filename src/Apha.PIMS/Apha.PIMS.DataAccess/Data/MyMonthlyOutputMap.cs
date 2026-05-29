using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PIMS.DataAccess.Data
{
    public class MyMonthlyOutputMap : IEntityTypeConfiguration<MyMonthlyOutput>
    {
        public void Configure(EntityTypeBuilder<MyMonthlyOutput> entity)
        {
            entity.HasKey(e => new { e.Year, e.Testcode, e.Buyer, e.Month, e.Workgroup }).HasName("pk_my_monthlyoutput");

            entity.ToTable("my_monthlyoutput", "mabarchive");

            entity.HasIndex(e => e.Month, "idx_my_monthlyoutput_month");

            entity.HasIndex(e => e.Testcode, "idx_my_monthlyoutput_testcode");

            entity.HasIndex(e => e.Year, "idx_my_monthlyoutput_year");

            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.Testcode)
                .HasMaxLength(20)
                .HasColumnName("testcode");
            entity.Property(e => e.Buyer)
                .HasMaxLength(20)
                .HasColumnName("buyer");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.Workgroup)
                .HasMaxLength(50)
                .HasColumnName("workgroup");
            entity.Property(e => e.Volume).HasColumnName("volume");
            entity.Property(e => e.Wgbuyer)
                .HasMaxLength(50)
                .HasColumnName("wgbuyer");
        }
    }
}
