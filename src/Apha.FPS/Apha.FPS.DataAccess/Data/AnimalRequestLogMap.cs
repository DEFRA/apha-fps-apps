using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class AnimalRequestLogMap : IEntityTypeConfiguration<AnimalRequestLog>
    {


        public void Configure(EntityTypeBuilder<AnimalRequestLog> entity)
        {
            entity.HasKey(e => new { e.SequenceNo, e.FpsYear }).HasName("pk_animalreq_log");

            entity.ToTable("animalreq_log", "fps");

            entity.HasIndex(e => e.DateTime, "animalreq_log_ind_dt");
            entity.HasIndex(e => e.JobCode, "animalreq_log_ind_jc").UseCollation(new[] { "latin1_general_ci_as" });

            entity.Property(e => e.SequenceNo)
                .ValueGeneratedOnAdd()
                .HasColumnName("sequenceno");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.AnimalType)
                .HasMaxLength(50)
                .UseCollation("latin1_general_ci_as")
                .HasColumnName("animaltype");
            entity.Property(e => e.DateTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_time");
            entity.Property(e => e.InsertDelete)
                .HasMaxLength(2)
                .IsFixedLength()
                .UseCollation("latin1_general_ci_as")
                .HasColumnName("insert_delete");
            entity.Property(e => e.JobCode)
                .HasMaxLength(20)
                .UseCollation("latin1_general_ci_as")
                .HasColumnName("jobcode");
            entity.Property(e => e.NumberOfAnimals).HasColumnName("numberofanimals");
            entity.Property(e => e.NumberOfDays).HasColumnName("numberofdays");
            entity.Property(e => e.UserId)
                .HasMaxLength(255)
                .UseCollation("latin1_general_ci_as")
                .HasColumnName("user_id");
        }
    }
}
