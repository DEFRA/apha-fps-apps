using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class ArchiveYearMap : IEntityTypeConfiguration<ArchiveYear>
    {
        public void Configure(EntityTypeBuilder<ArchiveYear> builder)
        {
            builder.HasKey(e => e.Year).HasName("pk_tlkpyear");
            builder.ToTable("tlkpyear", "mabarchive", t => t.ExcludeFromMigrations());
            builder.Property(e => e.Year).HasColumnName("year").ValueGeneratedNever();
            builder.Property(e => e.LatestMonthReleased).HasColumnName("latestmonthreleased");
        }
    }
}
