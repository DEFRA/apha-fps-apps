using Apha.Costbook.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.Costbook.DataAccess.Data
{
    public class WorkGroupGradeMap : IEntityTypeConfiguration<WorkGroupGrade>
    {
        public void Configure(EntityTypeBuilder<WorkGroupGrade> entity)
        {
            entity.HasKey(e => new { e.WgGrade, e.FpsYear }).HasName("pk_workgroupgrade");

            entity.ToTable("workgroupgrade", DbConstants.FpsSchemaName);

            entity.Property(e => e.WgGrade)
                .HasMaxLength(50)
                .HasColumnName("wggrade");
            entity.Property(e => e.AvSalary)
                .HasDefaultValueSql("0")
                .HasColumnType(DbConstants.MoneyColumnType)
                .HasColumnName("avsalary");
            entity.Property(e => e.ChargeRateWg)
                .HasColumnType(DbConstants.MoneyColumnType)
                .HasColumnName("chargeratewg");
            entity.Property(e => e.DirectRateWg)
                .HasDefaultValueSql("0")
                .HasColumnType(DbConstants.MoneyColumnType)
                .HasColumnName("directratewg");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.GradeCode)
                .HasMaxLength(10)
                .HasColumnName("gradecode");
            entity.Property(e => e.HrsChangedBy)
                .HasMaxLength(50)
                .HasColumnName("hrschangedby");
            entity.Property(e => e.NprWg)
                .HasDefaultValueSql("0")
                .HasColumnType(DbConstants.MoneyColumnType)
                .HasColumnName("nprwg");
            entity.Property(e => e.OhrWg)
                .HasDefaultValueSql("0")
                .HasColumnType(DbConstants.MoneyColumnType)
                .HasColumnName("ohrwg");
            entity.Property(e => e.PayRateWg)
                .HasDefaultValueSql("0")
                .HasColumnType(DbConstants.MoneyColumnType)
                .HasColumnName("payratewg");
            entity.Property(e => e.ProfitCentreGrade)
                .HasMaxLength(20)
                .HasColumnName("profitcentregrade");
            entity.Property(e => e.WorkGroup)
                .HasMaxLength(50)
                .HasColumnName("workgroup");

            
        }
    }
}
