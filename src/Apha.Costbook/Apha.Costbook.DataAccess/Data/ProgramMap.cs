using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.Costbook.DataAccess.Data
{
    public class ProgramMap : IEntityTypeConfiguration<Program>
    {
        public void Configure(EntityTypeBuilder<Program> entity)
        {
            entity.HasKey(e => new { e.ProgramNo, e.FpScalYear }).HasName("pk_tlkpprogram");

            entity.ToTable("tlkpprogram", "fps");

            entity.HasIndex(e => e.Minim, "tlkpprogram_minim");

            entity.Property(e => e.ProgramNo)
                .HasMaxLength(10)
                .HasColumnName("programno");
            entity.Property(e => e.FpScalYear).HasColumnName("fpsyear");
            entity.Property(e => e.Customer)
                .HasMaxLength(50)
                .HasColumnName("customer");
            entity.Property(e => e.Directorate)
                .HasMaxLength(15)
                .HasColumnName("directorate");
            entity.Property(e => e.Manager)
                .HasMaxLength(50)
                .HasColumnName("manager");
            entity.Property(e => e.Minim)
                .HasMaxLength(7)
                .HasColumnName("minim");
            entity.Property(e => e.ProgramName)
                .HasMaxLength(80)
                .HasColumnName("programname");
            entity.Property(e => e.SectorName)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Charge'::character varying")
                .HasColumnName("sector_name");
            entity.Property(e => e.Target)
                .HasPrecision(19, 4)
                .HasDefaultValue(0m)
                .HasColumnName("target");
        }
    }
}

