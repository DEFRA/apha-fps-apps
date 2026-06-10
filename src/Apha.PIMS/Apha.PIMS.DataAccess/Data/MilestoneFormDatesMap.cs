using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.DataAccess.Data
{
    public class MilestoneFormDatesMap : IEntityTypeConfiguration<MilestoneFormDates>
    {
        public void Configure(EntityTypeBuilder<MilestoneFormDates> entity)
        {
            entity.HasKey(e => new { e.Year, e.ParentProject }).HasName("pk_my_milestoneformdates");

            entity.ToTable("my_milestoneformdates", "mabarchive");

            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.ParentProject)
                .HasMaxLength(20)
                .HasColumnName("parentproject");
            entity.Property(e => e.Apr)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("apr");
            entity.Property(e => e.Aug)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("aug");
            entity.Property(e => e.Dec)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dec");
            entity.Property(e => e.Feb)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("feb");
            entity.Property(e => e.Jan)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("jan");
            entity.Property(e => e.Jul)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("jul");
            entity.Property(e => e.Jun)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("jun");
            entity.Property(e => e.Mar)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("mar");
            entity.Property(e => e.May)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("may");
            entity.Property(e => e.Nov)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("nov");
            entity.Property(e => e.Oct)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("oct");
            entity.Property(e => e.Sep)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("sep");
        }
    }
}
