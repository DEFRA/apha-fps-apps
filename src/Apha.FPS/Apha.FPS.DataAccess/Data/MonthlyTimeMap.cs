using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class MonthlyTimeMap : IEntityTypeConfiguration<MonthlyTime>
    {
        public void Configure(EntityTypeBuilder<MonthlyTime> entity)
        {
            entity.HasKey(e => new { e.PactStaffId, e.TimeCode, e.Month, e.ParentProject })
                .HasName("pk_monthlytime");

            entity.ToTable("monthlytime", "fps");

            entity.Property(e => e.PactStaffId).HasColumnType("citext").HasColumnName("pactstaffid");
            entity.Property(e => e.TimeCode).HasColumnType("citext").HasColumnName("timecode");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.ParentProject).HasColumnType("citext").HasColumnName("parentproject");
            entity.Property(e => e.WorkGroup).HasColumnType("citext").HasColumnName("workgroup");
            entity.Property(e => e.Hours).HasColumnName("hours");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
        }
    }
}
