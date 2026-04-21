using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class StaffJobLogMap : IEntityTypeConfiguration<StaffJobLog>
    {


        public void Configure(EntityTypeBuilder<StaffJobLog> entity)
        {
            entity.HasKey(e => new { e.SequenceNo, e.FpsYear }).HasName("pk_staffjob_log");

            entity.ToTable("staffjob_log", "fps");

            entity.HasIndex(e => e.DateTime, "staffjob_log_ind_dt");
            entity.HasIndex(e => e.JobCode, "staffjob_log_ind_jc");
            entity.HasIndex(e => e.SequenceNo, "staffjob_log_pk_idx");

            entity.Property(e => e.SequenceNo)
                .ValueGeneratedOnAdd()
                .HasColumnName("sequenceno");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.DateTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_time");
            entity.Property(e => e.InsertDelete)
                .HasMaxLength(2)
                .IsFixedLength()
                .HasColumnName("insert_delete");
            entity.Property(e => e.JobCode)
                .HasMaxLength(20)
                .HasColumnName("jobcode");
            entity.Property(e => e.PlannedHours).HasColumnName("plannedhours");
            entity.Property(e => e.StaffId)
                .HasMaxLength(50)
                .HasColumnName("staffid");
            entity.Property(e => e.UserId)
                .HasMaxLength(255)
                .HasColumnName("user_id");
        }
    }
}
