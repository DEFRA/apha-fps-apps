using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class BidMap : IEntityTypeConfiguration<Bid>
    {
        public void Configure(EntityTypeBuilder<Bid> builder)
        {
            builder.HasKey(e => new { e.WorkgroupName, e.Account, e.FpsYear }).HasName("pk_tblbid");

            builder.ToTable("tblbid", "fps");

            builder.Property(e => e.WorkgroupName)
                .HasMaxLength(50)
                .HasColumnName("workgroup");

            builder.Property(e => e.Account)
                .HasMaxLength(50)
                .HasColumnName("account");

            builder.Property(e => e.GenBid)
                .HasDefaultValueSql("0")
                .HasColumnType("money")
                .HasColumnName("genbid");

            builder.Property(e => e.FpsYear).HasColumnName("fpsyear");
        }
    }
}
