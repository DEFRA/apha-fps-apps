using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.DataAccess.Data
{
    public class ProjectDetailMap : IEntityTypeConfiguration<ProjectDetail>
    {
        public void Configure(EntityTypeBuilder<ProjectDetail> entity)
        {
            entity.HasNoKey();
            entity.Property(e => e.Parentproject).HasMaxLength(20).HasColumnName("parentproject");
            entity.Property(e => e.Version).HasMaxLength(20).HasColumnName("version");
            entity.Property(e => e.FileRef).HasMaxLength(50).HasColumnName("fileref");
            entity.Property(e => e.CustomerRef).HasMaxLength(50).HasColumnName("customerref");
            entity.Property(e => e.StartDate).HasColumnName("startdate");
            entity.Property(e => e.EndDate).HasColumnName("enddate");
            entity.Property(e => e.CostbookNumber).HasMaxLength(50).HasColumnName("costbooknumber");
            entity.Property(e => e.Riskid).HasColumnName("riskid");
            entity.Property(e => e.UseProjectYears).HasColumnName("useprojectyears");
            entity.Property(e => e.RevisedEndDate).HasColumnName("revisedenddate");
            entity.Property(e => e.ClosedDate).HasColumnName("closeddate");
        }
    }
}
