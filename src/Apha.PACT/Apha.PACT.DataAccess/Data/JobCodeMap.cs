using Apha.PACT.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PACT.DataAccess.Data
{
    public class JobCodeMap : IEntityTypeConfiguration<JobCode>
    {
        public void Configure(EntityTypeBuilder<JobCode> entity)
        {
            entity.HasKey(e => new { e.JobCodeId, e.FpsYear }).HasName("pk_tlkpjobcode");

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
                .HasColumnType("citext")
                .HasColumnName("parentproject");
            entity.Property(e => e.Type)
                .HasMaxLength(15)
                .HasColumnName("type");
        }
    }
}
