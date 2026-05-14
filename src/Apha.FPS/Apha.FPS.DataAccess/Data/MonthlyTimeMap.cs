using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class MonthlyTimeMap : IEntityTypeConfiguration<MonthlyTime>
    {
        public void Configure(EntityTypeBuilder<MonthlyTime> builder)
        {
            builder.ToTable("monthlytime", "fps");

            builder.HasKey(e => new { e.PactStaffId, e.TimeCode, e.Month, e.ParentProject, e.FpsYear })
                   .HasName("pk_monthlytime");

            builder.Property(e => e.PactStaffId)
                .HasMaxLength(50)
                .HasColumnName("pactstaffid");
            builder.Property(e => e.TimeCode)
                .HasMaxLength(50)
                .HasColumnName("timecode");
            builder.Property(e => e.Month)
                .HasColumnName("month");
            builder.Property(e => e.ParentProject)
                .HasMaxLength(20)
                .HasColumnName("parentproject");
            builder.Property(e => e.FpsYear)
                .HasColumnName("fpsyear");
            builder.Property(e => e.Hours)
                .HasColumnName("hours");
            builder.Property(e => e.WorkGroup)
                .HasMaxLength(50)
                .HasColumnName("workgroup");
        }
    }
}
