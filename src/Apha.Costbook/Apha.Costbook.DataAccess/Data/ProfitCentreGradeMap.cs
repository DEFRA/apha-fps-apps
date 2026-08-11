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

            entity.ToTable("profitcentregrade", "fps");

            entity.HasIndex(e => e.ProfitCentre, "profitcentregrade_profitcentre")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.Property(e => e.PcGrade)
                .HasMaxLength(20)
                .HasColumnName("pcgrade");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.ChargeRate)
                .HasPrecision(19, 4)
                .HasColumnName("chargerate");
            entity.Property(e => e.DefraChargeRate)
                .HasPrecision(19, 4)
                .HasColumnName("defrachargerate");
            entity.Property(e => e.DirectRate)
                .HasPrecision(19, 4)
                .HasDefaultValue(0m)
                .HasColumnName("directrate");
            entity.Property(e => e.DivisionGrade)
                .HasMaxLength(10)
                .HasColumnName("divisiongrade");
            entity.Property(e => e.GradeCode)
                .HasMaxLength(10)
                .HasColumnName("gradecode");
            entity.Property(e => e.HrsAvailable)
                .HasDefaultValue(0.0)
                .HasColumnName("hrsavailable");
            entity.Property(e => e.Npr)
                .HasPrecision(19, 4)
                .HasDefaultValue(0m)
                .HasColumnName("npr");
            entity.Property(e => e.Ohr)
                .HasPrecision(19, 4)
                .HasDefaultValue(0m)
                .HasColumnName("ohr");
            entity.Property(e => e.OldChargeRate)
                .HasPrecision(19, 4)
                .HasDefaultValue(0m)
                .HasColumnName("oldchargerate");
            entity.Property(e => e.PayRate)
                .HasPrecision(19, 4)
                .HasDefaultValue(0m)
                .HasColumnName("payrate");
            entity.Property(e => e.ProfitCentre)
                .HasMaxLength(50)
                .HasColumnName("profitcentre");
        }
    }
}
