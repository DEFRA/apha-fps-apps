using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
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
                .HasColumnType("money")
                .HasColumnName("chargerate");
            entity.Property(e => e.DefraChargeRate)
                .HasColumnType("money")
                .HasColumnName("defrachargerate");
            entity.Property(e => e.DirectRate)
                .HasDefaultValueSql("0")
                .HasColumnType("money")
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
            entity.Property(e => e.NPR)
                .HasDefaultValueSql("0")
                .HasColumnType("money")
                .HasColumnName("npr");
            entity.Property(e => e.OHR)
                .HasDefaultValueSql("0")
                .HasColumnType("money")
                .HasColumnName("ohr");
            entity.Property(e => e.OldChargeRate)
                .HasDefaultValueSql("0")
                .HasColumnType("money")
                .HasColumnName("oldchargerate");
            entity.Property(e => e.PayRate)
                .HasDefaultValueSql("0")
                .HasColumnType("money")
                .HasColumnName("payrate");
            entity.Property(e => e.ProfitCentre)
                .HasMaxLength(50)
                .HasColumnName("profitcentre");
        }
    }
}
