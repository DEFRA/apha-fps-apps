using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class AnimalMap : IEntityTypeConfiguration<Animal>
    {


        public void Configure(EntityTypeBuilder<Animal> entity)
        {
            entity.HasKey(e => e.AnimalType).HasName("tblanimals_pk__tblanimals__18ebb532");

            entity.ToTable("tblanimals", "fps");

            entity.Property(e => e.AnimalType)
                .HasMaxLength(50)
                .HasColumnName("animaltype");
            entity.Property(e => e.DailyRate)
                .HasColumnType("money")
                .HasColumnName("dailyrate");
            entity.Property(e => e.DefraDailyRate)
                .HasColumnType("money")
                .HasColumnName("defradailyrate");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.PlanByWeek)
                .HasDefaultValue(false)
                .HasColumnName("planbyweek");
            entity.Property(e => e.SecurityLevel)
                .HasMaxLength(50)
                .HasColumnName("security_level");
            entity.Property(e => e.Species)
                .HasMaxLength(50)
                .HasColumnName("species");
        }
    }
}
