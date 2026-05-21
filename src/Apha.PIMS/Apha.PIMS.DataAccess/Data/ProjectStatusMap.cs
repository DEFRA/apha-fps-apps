using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.DataAccess.Data
{
    public class ProjectStatusMap : IEntityTypeConfiguration<ProjectStatus>
    {
        public void Configure(EntityTypeBuilder<ProjectStatus> entity)
        {
            entity.HasKey(e => e.Projectstatus).HasName("pk_tlkpprojectstatus");

            entity.ToTable("tlkpprojectstatus", "mabarchive");

            entity.Property(e => e.Projectstatus)
                .HasMaxLength(50)
                .HasColumnName("projectstatus");
            entity.Property(e => e.IsFps).HasColumnName("is_fps");
            entity.Property(e => e.IsPims).HasColumnName("is_pims");
        }
    }
}
