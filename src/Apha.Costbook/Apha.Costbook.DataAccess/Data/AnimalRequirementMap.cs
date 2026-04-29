using Apha.Costbook.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.Costbook.DataAccess.Data
{
    public class AnimalRequirementMap : IEntityTypeConfiguration<AnimalRequirement>
    {
        public void Configure(EntityTypeBuilder<AnimalRequirement> entity)
        {
            entity.HasKey(e => e.ArIdentity).HasName("aaaaatblanimalreq_pk");

            entity.ToTable("tblanimalreq", "mabarchive");

            entity.HasIndex(e => new { e.Project, e.Year, e.AnimalType }, "tblanimalreq_proj_ind");

            entity.HasIndex(e => e.Project, "tblanimalreq_tblanimalreqproject");

            entity.HasIndex(e => new { e.Project, e.Year }, "tblprojectyeartblanimalreq");

            entity.Property(e => e.ArIdentity).HasColumnName("ar_identity");
            entity.Property(e => e.AnimalType)
                .HasMaxLength(50)
                .HasColumnName("animaltype");
            entity.Property(e => e.DailyRate)
                .HasDefaultValueSql("0")
                .HasColumnName("dailyrate");
            entity.Property(e => e.NumberOfAnimals)
                .HasDefaultValueSql("0")
                .HasColumnName("number of animals");
            entity.Property(e => e.NumberOfDays).HasColumnName("number of days");
            entity.Property(e => e.Project)
                .HasMaxLength(50)
                .HasColumnName("project");
            entity.Property(e => e.Year)
                .HasDefaultValue(0)
                .HasColumnName("year");
        }
    }
}
