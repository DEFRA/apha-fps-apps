using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class TimeCodeValidMap : IEntityTypeConfiguration<TimeCodeValid>
    {
        public void Configure(EntityTypeBuilder<TimeCodeValid> entity)
        {
            entity.HasKey(e => new { e.WorkGroup, e.TimeCode, e.ParentProject })
                .HasName("aaaaatimecodevalid_pk");

            entity.ToTable("timecodevalid", "fps");

            entity.HasIndex(e => e.JobCode).HasDatabaseName("reference20");
            entity.HasIndex(e => new { e.TestCode, e.Portfolio }).HasDatabaseName("reference24");
            entity.HasIndex(e => e.ParentProject).HasDatabaseName("reference3");

            entity.Property(e => e.WorkGroup).HasColumnType("citext").HasColumnName("workgroup");
            entity.Property(e => e.TimeCode).HasColumnType("citext").HasColumnName("timecode");
            entity.Property(e => e.ParentProject).HasColumnType("citext").HasColumnName("parentproject");
            entity.Property(e => e.TestCode).HasMaxLength(50).HasColumnName("testcode");
            entity.Property(e => e.JobCode).HasMaxLength(50).HasColumnName("jobcode");
            entity.Property(e => e.Portfolio).HasMaxLength(20).HasColumnName("portfolio");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
        }
    }
}
