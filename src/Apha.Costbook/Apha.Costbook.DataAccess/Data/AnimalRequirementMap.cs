using Apha.Costbook.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.Costbook.DataAccess.Data
{
    public class AnimalRequirementMap : IEntityTypeConfiguration<AnimalRequirement>
    {
        public void Configure(EntityTypeBuilder<AnimalRequirement> entity)
        {
            entity.HasKey(e => e.ArIdentity).HasName("pk_tblanimalreq");

            entity.ToTable("tblanimalreq", DbConstants.MabArchiveSchemaName);

            entity.HasIndex(e => e.Project, "idx_tblanimalreq_project");

            entity.HasIndex(e => new { e.Project, e.Year }, "idx_tblanimalreq_project_year");

            entity.HasIndex(e => new { e.Project, e.Year, e.AnimalType }, "idx_tblanimalreq_project_year_animaltype");

            entity.Property(e => e.ArIdentity).HasColumnName("ar_identity");
            entity.Property(e => e.AnimalType)
                .HasMaxLength(50)
                .HasColumnName("animaltype");
            entity.Property(e => e.DailyRate)
                .HasDefaultValueSql("0")
                .HasColumnName("dailyrate");
            entity.Property(e => e.NumberOfAnimals)
                .HasDefaultValueSql("0")
                .HasColumnName("number_of_animals");
            entity.Property(e => e.NumberOfDays)
                .HasDefaultValueSql("0")
                .HasColumnName("number_of_days");
            entity.Property(e => e.Project)
                .HasMaxLength(50)
                .HasColumnName("project");
            entity.Property(e => e.Year)
                .HasDefaultValue(0)
                .HasColumnName("year");
        }
    }
}
