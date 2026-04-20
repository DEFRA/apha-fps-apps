using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PACT.DataAccess.Data
{
    public class TestRequirementLogMap : IEntityTypeConfiguration<TestRequirementLog>
    {
        public void Configure(EntityTypeBuilder<TestRequirementLog> entity)
        {
            entity.HasKey(e => new { e.SequenceNo, e.FpsYear }).HasName("pk_testreq_log");

            entity.ToTable("testreq_log", "fps");

            entity.HasIndex(e => e.SequenceNo, "idx_testreqlog_sequenceno")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.HasIndex(e => e.DateTime, "testreq_log_ind_dt");

            entity.HasIndex(e => e.JobCode, "testreq_log_ind_jc");

            entity.Property(e => e.SequenceNo)
                .ValueGeneratedOnAdd()
                .HasColumnName("sequenceno");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.Buyer)
                .HasMaxLength(20)
                .UseCollation("latin1_general_ci_as")
                .HasColumnName("buyer");
            entity.Property(e => e.DateTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_time");
            entity.Property(e => e.InsertDelete)
                .HasMaxLength(2)
                .IsFixedLength()
                .UseCollation("latin1_general_ci_as")
                .HasColumnName("insert_delete");
            entity.Property(e => e.JobCode)
                .HasMaxLength(50)
                .HasComment("Generated column based on projectbuyercode")
                .HasColumnName("jobcode");
            entity.Property(e => e.NoRequired).HasColumnName("norequired");
            entity.Property(e => e.ProjectBuyerCode)
                .HasMaxLength(50)
                .UseCollation("latin1_general_ci_as")
                .HasColumnName("projectbuyercode");
            entity.Property(e => e.TestBuyerCode)
                .HasMaxLength(50)
                .UseCollation("latin1_general_ci_as")
                .HasColumnName("testbuyercode");
            entity.Property(e => e.TestCode)
                .HasMaxLength(20)
                .HasColumnName("testcode");
            entity.Property(e => e.UnitPrice).HasColumnName("unitprice");
            entity.Property(e => e.UserId)
                .HasMaxLength(255)
                .UseCollation("latin1_general_ci_as")
                .HasColumnName("user_id");
        }
    }
}
