using Apha.PACT.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PACT.DataAccess.Data
{
    public class MonthlyTimeLogMap : IEntityTypeConfiguration<MonthlyTimeLog>
    {
        public void Configure(EntityTypeBuilder<MonthlyTimeLog> entity)
        {
            entity.HasKey(e => new { e.SequenceNo, e.FpsYear }).HasName("pk_mt_log");

            entity.ToTable("mt_log", "fps");

            entity.Property(e => e.SequenceNo)
                .ValueGeneratedOnAdd()
                .HasColumnName("sequenceno");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.DateTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_time");
            entity.Property(e => e.Hours).HasColumnName("hours");
            entity.Property(e => e.InsertDelete)
                .HasMaxLength(2)
                .IsFixedLength()
                .HasColumnName("insert_delete");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.PactStaffId)
                .HasMaxLength(50)
                .HasColumnName("pactstaffid");
            entity.Property(e => e.ParentProject)
                .HasMaxLength(20)
                .HasColumnName("parentproject");
            entity.Property(e => e.TimeCode)
                .HasMaxLength(50)
                .HasColumnName("timecode");
            entity.Property(e => e.UserId)
                .HasMaxLength(255)
                .HasColumnName("user_id");
            entity.Property(e => e.WorkGroup)
                .HasMaxLength(50)
                .HasColumnName("workgroup");
        }
    }
}
