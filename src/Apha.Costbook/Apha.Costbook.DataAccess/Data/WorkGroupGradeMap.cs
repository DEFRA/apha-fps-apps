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

            entity.ToTable("workgroupgrade", "fps");

            entity.Property(e => e.WgGrade)
                .HasMaxLength(50)
                .HasColumnName("wggrade");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.AvSalary)
                .HasPrecision(19, 4)
                .HasDefaultValue(0m)
                .HasColumnName("avsalary");
            entity.Property(e => e.ChargeRateWg)
                .HasPrecision(19, 4)
                .HasColumnName("chargeratewg");
            entity.Property(e => e.DirectRateWg)
                .HasPrecision(19, 4)
                .HasDefaultValue(0m)
                .HasColumnName("directratewg");
            entity.Property(e => e.GradeCode)
                .HasMaxLength(10)
                .HasColumnName("gradecode");
            entity.Property(e => e.HrsChangedBy)
                .HasMaxLength(50)
                .HasColumnName("hrschangedby");
            entity.Property(e => e.NprWg)
                .HasPrecision(19, 4)
                .HasDefaultValue(0m)
                .HasColumnName("nprwg");
            entity.Property(e => e.OhrWg)
                .HasPrecision(19, 4)
                .HasDefaultValue(0m)
                .HasColumnName("ohrwg");
            entity.Property(e => e.PayRateWg)
                .HasPrecision(19, 4)
                .HasDefaultValue(0m)
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
