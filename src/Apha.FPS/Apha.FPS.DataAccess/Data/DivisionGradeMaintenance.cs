using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class DivisionGradeMaintenanceMap : IEntityTypeConfiguration<DivisionGradeMaintenance>
    {
        public void Configure(EntityTypeBuilder<DivisionGradeMaintenance> entity)
        {
            entity.HasKey(e => new { e.DivisionGradeCode, e.FpsYear }).HasName("pk_divisiongrade");

            entity.ToTable("divisiongrade", "fps");

            entity.Property(e => e.DivisionGradeCode)
                .HasMaxLength(10)
                .HasColumnName("divisiongrade");

            entity.Property(e => e.FpsYear)
                .HasColumnName("fpsyear");

            entity.Property(e => e.ChargeRate)
                .HasDefaultValueSql("0")
                .HasColumnType("money")
                .HasColumnName("chargerate");

            entity.Property(e => e.DirectRate)
                .HasDefaultValueSql("0")
                .HasColumnType("money")
                .HasColumnName("directrate");

            entity.Property(e => e.Division)
                .HasMaxLength(10)
                .HasColumnName("division");

            entity.Property(e => e.GradeCode)
                .HasMaxLength(10)
                .HasColumnName("gradecode");

            entity.Property(e => e.Npr)
                .HasDefaultValueSql("0")
                .HasColumnType("money")
                .HasColumnName("npr");

            entity.Property(e => e.Ohr)
                .HasDefaultValueSql("0")
                .HasColumnType("money")
                .HasColumnName("ohr");

            entity.Property(e => e.PayRate)
                .HasDefaultValueSql("0")
                .HasColumnType("money")
                .HasColumnName("payrate");
        }
    }
}
