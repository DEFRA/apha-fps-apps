using Apha.PACT.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PACT.DataAccess.Data
{
    public class StagingMonthlyTimeMap : IEntityTypeConfiguration<StagingMonthlyTime>
    {
        public void Configure(EntityTypeBuilder<StagingMonthlyTime> entity)
        {
            entity.HasKey(e => e.Id).HasName("pk_tblstagingmonthlytime");

            entity.ToTable("tblstagingmonthlytime", "fps");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FailureComments)
                .HasColumnType("character varying")
                .HasColumnName("failurecomments");
            entity.Property(e => e.Hours).HasColumnName("hours");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.Name)
                .HasColumnName("name");
            entity.Property(e => e.NewWorkGroup)
                .HasColumnName("newworkgroup");
            entity.Property(e => e.OldTestCode)
                .HasColumnName("oldtestcode");
            entity.Property(e => e.PactId)
                .HasColumnName("pactid");
            entity.Property(e => e.PactStaffId)
                .HasColumnName("pactstaffid");
            entity.Property(e => e.ParentProject)
                .HasColumnName("parentproject");
            entity.Property(e => e.Passed).HasColumnName("passed");
            entity.Property(e => e.TimeCode)
                .HasColumnName("timecode");
            entity.Property(e => e.WorkGroup)
                .HasColumnName("workgroup");
            entity.Property(e => e.Filename)
                .HasMaxLength(255)
                .HasColumnName("filename");
            entity.Property(e => e.ImportedBy)
                .HasMaxLength(255)
                .HasColumnName("importedby");
            entity.Property(e => e.ImportedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("importeddate");
        }
    }
}
