using Apha.Costbook.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.Costbook.DataAccess.Data
{
    public class ProfitCentreGradeMap : IEntityTypeConfiguration<ProfitCentreGrade>
    {
        public void Configure(EntityTypeBuilder<ProfitCentreGrade> entity)
        {
            entity.HasKey(e => new { e.PcGrade, e.FpsYear }).HasName("pk_profitcentregrade");

            entity.ToTable("profitcentregrade", DbConstants.FpsSchemaName);

            entity.HasIndex(e => e.ProfitCentre, "profitcentregrade_profitcentre");

            entity.Property(e => e.PcGrade)
                .HasColumnType(DbConstants.CitextColumnType)
                .HasColumnName("pcgrade");
            entity.Property(e => e.FpsYear).HasColumnName(DbConstants.FpsYearColumnName);
            entity.Property(e => e.ChargeRate)
                .HasColumnType(DbConstants.MoneyColumnType)
                .HasColumnName("chargerate");
            entity.Property(e => e.DefraChargeRate)
                .HasColumnType(DbConstants.MoneyColumnType)
                .HasColumnName("defrachargerate");
            entity.Property(e => e.DirectRate)
                .HasDefaultValueSql("0")
                .HasColumnType(DbConstants.MoneyColumnType)
                .HasColumnName("directrate");
            entity.Property(e => e.DivisionGrade)
                .HasColumnType(DbConstants.CitextColumnType)
                .HasColumnName("divisiongrade");
            entity.Property(e => e.GradeCode)
                .HasColumnType(DbConstants.CitextColumnType)
                .HasColumnName("gradecode");
            entity.Property(e => e.HrsAvailable)
                .HasDefaultValueSql("0")
                .HasColumnName("hrsavailable");
            entity.Property(e => e.Npr)
                .HasDefaultValueSql("0")
                .HasColumnType(DbConstants.MoneyColumnType)
                .HasColumnName("npr");
            entity.Property(e => e.Ohr)
                .HasDefaultValueSql("0")
                .HasColumnType(DbConstants.MoneyColumnType)
                .HasColumnName("ohr");
            entity.Property(e => e.OldChargeRate)
                .HasDefaultValueSql("0")
                .HasColumnType(DbConstants.MoneyColumnType)
                .HasColumnName("oldchargerate");
            entity.Property(e => e.PayRate)
                .HasDefaultValueSql("0")
                .HasColumnType(DbConstants.MoneyColumnType)
                .HasColumnName("payrate");
            entity.Property(e => e.ProfitCentre)
                .HasColumnType(DbConstants.CitextColumnType)
                .HasColumnName("profitcentre");
        }
    }
}
