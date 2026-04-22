using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.Costbook.DataAccess.Data
{
    public class FpsAnimalsMap : IEntityTypeConfiguration<FpsAnimals>
    {
        public void Configure(EntityTypeBuilder<FpsAnimals> entity)
        {
            entity.HasKey(e => new { e.AnimalType, e.FpsYear }).HasName("pk_tblanimals");

            entity.ToTable("tblanimals", "fps");

            entity.Property(e => e.AnimalType)
                .HasColumnType("citext")
                .HasColumnName("animaltype");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.DailyRate)
                .HasColumnType("money")
                .HasColumnName("dailyrate");
            entity.Property(e => e.DefraDailyRate)
                .HasColumnType("money")
                .HasColumnName("defradailyrate");
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
