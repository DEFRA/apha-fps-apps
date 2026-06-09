using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.DataAccess.Data
{
    public class MilestoneMap : IEntityTypeConfiguration<Milestone>
    {
        public void Configure(EntityTypeBuilder<Milestone> entity)
        {
            entity.HasKey(e => new { e.Project, e.Number }).HasName("pk_tblmilestone");

            entity.ToTable("tblmilestone", "mabarchive");

            entity.Property(e => e.Project)
                .HasMaxLength(20)
                .HasColumnName("project");
            entity.Property(e => e.Number)
                .HasMaxLength(10)
                .HasColumnName("number");
            entity.Property(e => e.CapsComment)
                .HasMaxLength(250)
                .HasColumnName("capscomment");
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
            entity.Property(e => e.OnTarget)
                .HasDefaultValue((short)0)
                .HasColumnName("ontarget");
            entity.Property(e => e.ProjectLeaderComment)
                .HasColumnType("character varying")
                .HasColumnName("projectleadercomment");
            entity.Property(e => e.UnderSdReview)
                .HasDefaultValue((short)0)
                .HasColumnName("undersdreview");
        }
    }
}
