using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class JobCodeMap : IEntityTypeConfiguration<JobCode>
    {


        public void Configure(EntityTypeBuilder<JobCode> entity)
        {
            entity.HasKey(e => e.JobCodeId).HasName("tlkpjobcode_pk_tlkpjobcode_new_1__15");

            entity.ToTable("tlkpjobcode", "fps");

            entity.Property(e => e.JobCodeId)
                .HasMaxLength(50)
                .HasColumnName("jobcode");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.JobCodeName)
                .HasMaxLength(255)
                .HasColumnName("jobcodename");
            entity.Property(e => e.JobCodeWorkGroup)
                .HasMaxLength(50)
                .HasColumnName("jobcodeworkgroup");
            entity.Property(e => e.NewProg)
                .HasMaxLength(20)
                .HasColumnName("newprog");
            entity.Property(e => e.ParentProject)
                .HasMaxLength(20)
                .HasColumnName("parentproject");
            entity.Property(e => e.Type)
                .HasMaxLength(15)
                .HasColumnName("type");
        }
    }
}
