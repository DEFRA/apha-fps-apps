using Apha.PACT.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PACT.DataAccess.Data
{
    public class MonthlyTimeMap : IEntityTypeConfiguration<MonthlyTime>
    {
        public void Configure(EntityTypeBuilder<MonthlyTime> entity)
        {
            entity.HasKey(e => new { e.PactStaffId, e.TimeCode, e.Month, e.ParentProject, e.FpsYear }).HasName("pk_monthlytime");

            entity.ToTable("monthlytime", "fps");

            entity.HasIndex(e => e.PactStaffId, "ijnd_staffid");

            entity.HasIndex(e => e.WorkGroup, "monthlytime_workgroup");

            entity.HasIndex(e => new { e.WorkGroup, e.TimeCode, e.ParentProject }, "reference23");

            entity.HasIndex(e => e.TimeCode, "timecode");

            entity.Property(e => e.PactStaffId)
                .HasColumnType("citext")
                .HasColumnName("pactstaffid");
            entity.Property(e => e.TimeCode)
                .HasColumnType("citext")
                .HasColumnName("timecode");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.ParentProject)
                .HasColumnType("citext")
                .HasColumnName("parentproject");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.Hours).HasColumnName("hours");
            entity.Property(e => e.WorkGroup)
                .HasColumnType("citext")
                .HasColumnName("workgroup");
        }
    }
}
