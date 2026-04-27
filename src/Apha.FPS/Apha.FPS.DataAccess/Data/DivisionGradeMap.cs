using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class DivisionGradeMap : IEntityTypeConfiguration<DivisionGrade>
    {
        private const string Latin1GeneralCiAs = "latin1_general_ci_as";

        public void Configure(EntityTypeBuilder<DivisionGrade> entity)
        {
            entity.HasKey(e => new { e.Division, e.FpsYear }).HasName("pk_divisiongrade");

            entity.ToTable("divisiongrade", "fps", tb => tb.HasComment("Division grade mapping table linking divisions to grade codes."));

            entity.Property(e => e.Division)
                .HasMaxLength(255)
                .HasComment("Division name (foreign key to fps.tlkpdivision.divname).")
                .HasColumnName("division");

            entity.Property(e => e.GradeCode)
                .HasMaxLength(50)
                .UseCollation(Latin1GeneralCiAs)
                .HasComment("Grade code identifier.")
                .HasColumnName("gradecode");

            entity.Property(e => e.FpsYear)
                .HasComment("Fiscal year.")
                .HasColumnName("fpsyear");
        }
    }
}
