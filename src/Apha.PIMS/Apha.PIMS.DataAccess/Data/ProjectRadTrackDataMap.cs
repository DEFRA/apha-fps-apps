using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.DataAccess.Data
{
    public class ProjectRadTrackDataMap : IEntityTypeConfiguration<ProjectRadTrackData>
    {
        public void Configure(EntityTypeBuilder<ProjectRadTrackData> entity)
        {
            entity.HasKey(e => e.Parentproject).HasName("pk_g_tlkpproject_radtrackdata");
            entity.ToTable("g_tlkpproject_radtrackdata", "mabarchive");
            entity.Property(e => e.Parentproject).HasMaxLength(20).HasColumnName("parentproject");
            entity.Property(e => e.Version).HasMaxLength(20).HasColumnName("version");
            entity.Property(e => e.Fileref).HasMaxLength(50).HasColumnName("fileref");
            entity.Property(e => e.Customerref).HasMaxLength(50).HasColumnName("customerref");
            entity.Property(e => e.Startdate).HasColumnName("startdate");
            entity.Property(e => e.Enddate).HasColumnName("enddate");
            entity.Property(e => e.Finalreportreceived).HasColumnName("finalreportreceived");
            entity.Property(e => e.Finalreportsent).HasColumnName("finalreportsent");
            entity.Property(e => e.Inflation).HasColumnName("inflation");
            entity.Property(e => e.Closeddate).HasColumnName("closeddate");
            entity.Property(e => e.Useprojectyear).HasColumnName("useprojectyear");
            entity.Property(e => e.Status).HasMaxLength(50).HasColumnName("status");
            entity.Property(e => e.Pcforecastspend).HasColumnName("pcforecastspend");
            entity.Property(e => e.Riskid).HasColumnName("riskid");
            entity.Property(e => e.Costbooknumber).HasMaxLength(50).HasColumnName("costbooknumber");
            entity.Property(e => e.Revisedenddate).HasColumnName("revisedenddate");
            entity.Property(e => e.Formrequired).HasColumnName("formrequired");
            entity.Property(e => e.Overallcustincome).HasColumnName("overallcustincome");
        }
    }
}
