using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class ProfitCentreGradeMap : IEntityTypeConfiguration<ProfitCentreGrade>
    {


        public void Configure(EntityTypeBuilder<ProfitCentreGrade> entity)
        {
            entity.HasKey(e => e.PcGrade).HasName("profitcentregrade_pk__profitcentregrad__2bde8e15");

            entity.ToTable("profitcentregrade", "fps");

            entity.Property(e => e.PcGrade)
                .HasMaxLength(20)
                .HasColumnName("pcgrade");
            entity.Property(e => e.ChargeRate).HasColumnName("chargerate");
            entity.Property(e => e.DefraChargeRate).HasColumnName("defrachargerate");
            entity.Property(e => e.DirectRate)
                .HasDefaultValueSql("0")
                .HasColumnType("money")
                .HasColumnName("directrate");
            entity.Property(e => e.DivisionGrade)
                .HasMaxLength(50)
                .UseCollation("latin1_general_ci_as")
                .HasColumnName("divisiongrade");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.GradeCode)
                .HasMaxLength(50)
                .UseCollation("latin1_general_ci_as")
                .HasColumnName("gradecode");
            entity.Property(e => e.HrsAvailable)
                .HasDefaultValueSql("0")
                .HasColumnName("hrsavailable");
            entity.Property(e => e.NPR)
                .HasDefaultValueSql("0")
                .HasColumnType("money")
                .HasColumnName("npr");
            entity.Property(e => e.OHR)
                .HasColumnType("money")
                .HasColumnName("ohr");
            entity.Property(e => e.OldChargeRate)
                .HasColumnType("money")
                .HasColumnName("oldchargerate");
            entity.Property(e => e.PayRate)
                .HasColumnType("money")
                .HasColumnName("payrate");
            entity.Property(e => e.ProfitCentre)
                .HasMaxLength(50)
                .HasColumnName("profitcentre");
        }
    }
}
