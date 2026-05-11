using Apha.Costbook.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.Costbook.DataAccess.Data
{
    public class ProjectMap : IEntityTypeConfiguration<Project>
    {
        public void Configure(EntityTypeBuilder<Project> entity)
        {
            entity.HasKey(e => e.ProjectId).HasName("tblproject_aaaaatblproject_pk");

            entity.ToTable("tblproject", DbConstants.MabArchiveSchemaName);

            entity.Property(e => e.ProjectId)
                .HasMaxLength(50)
                .HasColumnName("project");
            entity.Property(e => e.ContractNumber)
                .HasMaxLength(50)
                .HasColumnName("contract number");
            entity.Property(e => e.ContractPrice).HasColumnName("contractprice");
            entity.Property(e => e.CustomerName)
                .HasMaxLength(50)
                .HasColumnName("customer name");
            entity.Property(e => e.DateOfSubmission).HasColumnName("date of submission");
            entity.Property(e => e.Disease)
                .HasMaxLength(50)
                .HasColumnName("disease");
            entity.Property(e => e.Euroconvrate).HasColumnName("euroconvrate");
            entity.Property(e => e.FinancialYears).HasColumnName("financialyears");
            entity.Property(e => e.Inflation)
                .HasDefaultValue(0)
                .HasColumnName("inflation");
            entity.Property(e => e.IsDefraProject).HasColumnName("isdefraproject");
            entity.Property(e => e.Notes)
                .HasMaxLength(255)
                .HasColumnName("notes");
            entity.Property(e => e.PlanCategory)
                .HasMaxLength(50)
                .HasColumnName("plancat");
            entity.Property(e => e.PreparedBy)
                .HasMaxLength(50)
                .HasColumnName("prepared by");
            entity.Property(e => e.Programme)
                .HasMaxLength(50)
                .HasColumnName("programme");
            entity.Property(e => e.ProjectTitle)
                .HasMaxLength(100)
                .HasColumnName("projecttitle");
            entity.Property(e => e.ProjectWorkgroup)
                .HasMaxLength(50)
                .HasColumnName("projectworkgroup");
            entity.Property(e => e.StartDate).HasColumnName("startdate");
            entity.Property(e => e.StartFYear)
                .HasDefaultValueSql("0")
                .HasColumnName("startfyear");
            entity.Property(e => e.SubmittedByFName)
                .HasMaxLength(50)
                .HasColumnName("submittedbyfname");
            entity.Property(e => e.SubmittedByLName)
                .HasMaxLength(50)
                .HasColumnName("submittedbylname");
        }
    }
}
