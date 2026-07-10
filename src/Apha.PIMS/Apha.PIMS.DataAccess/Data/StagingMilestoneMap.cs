using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.DataAccess.Data
{
    public class StagingMilestoneMap : IEntityTypeConfiguration<StagingMilestone>
    {
        public void Configure(EntityTypeBuilder<StagingMilestone> entity)
        {
            entity
               .ToTable("tblstagingmilestone", "mabarchive");

            entity.HasKey(e => e.Id).HasName("pk_tblstagingmilestone_id");
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd().UseIdentityByDefaultColumn();

            entity.Property(e => e.AltDate).HasColumnName("alt_date");
            entity.Property(e => e.AltDescription).HasColumnName("alt_description");
            entity.Property(e => e.AltNumber).HasColumnName("alt_number");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(255)
                .HasColumnName("createdby");
            entity.Property(e => e.DateDue)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("datedue");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.Number)
                .HasMaxLength(10)
                .HasColumnName("number");
            entity.Property(e => e.Project)
                .HasMaxLength(20)
                .HasColumnName("project");
            entity.Property(e => e.TypeId)
                .HasMaxLength(5)
                .HasColumnName("typeid");
        }
    }
}
