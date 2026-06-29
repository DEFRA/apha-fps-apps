using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PIMS.DataAccess.Data
{
    public class LogMilestoneMap : IEntityTypeConfiguration<LogMilestone>
    {
        public void Configure(EntityTypeBuilder<LogMilestone> entity)
        {
            entity.HasKey(e => e.Id).HasName("pk_tbllogmilestone");

            entity.ToTable("tbllogmilestone", "mabarchive");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CapsComment)
                .HasMaxLength(250)
                .HasColumnName("capscomment");
            entity.Property(e => e.ChangedBy)
                .HasMaxLength(10)
                .HasColumnName("changedby");
            entity.Property(e => e.DateChanged)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("datechanged");
            entity.Property(e => e.DateCompleted)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("datecompleted");
            entity.Property(e => e.DateDue)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("datedue");
            entity.Property(e => e.DateFormReceived)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dateformreceived");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.IdType)
                .HasMaxLength(1)
                .HasColumnName("idtype");
            entity.Property(e => e.Number)
                .HasMaxLength(10)
                .HasColumnName("number");
            entity.Property(e => e.OnTarget).HasColumnName("ontarget");
            entity.Property(e => e.Project)
                .HasMaxLength(20)
                .HasColumnName("project");
            entity.Property(e => e.ProjectLeaderComment)
                .HasColumnType("character varying")
                .HasColumnName("projectleadercomment");
            entity.Property(e => e.UnderSdReview).HasColumnName("undersdreview");
            entity.Property(e => e.UpdateType)
                .HasMaxLength(1)
                .HasColumnName("updatetype");
        }
    }
}
