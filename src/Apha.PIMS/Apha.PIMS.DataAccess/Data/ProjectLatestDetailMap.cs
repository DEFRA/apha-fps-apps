using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.DataAccess.Data
{
    public class ProjectLatestDetailMap : IEntityTypeConfiguration<ProjectLatestDetail>
    {
        public void Configure(EntityTypeBuilder<ProjectLatestDetail> entity)
        {
            entity.HasNoKey();
            entity.ToView("vprojectlatestdetails", "mabarchive"); // Ensure the schema is correct
            entity.Property(e => e.ParentProject).HasColumnName("parentproject");
            entity.Property(e => e.Program).HasColumnName("program");
            entity.Property(e => e.Customer).HasColumnName("customer");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.ProjectGroup).HasColumnName("projectgroup");
        }
    }
}
