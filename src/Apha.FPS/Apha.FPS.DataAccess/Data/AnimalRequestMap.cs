using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class AnimalRequestMap : IEntityTypeConfiguration<AnimalRequest>
    {


        public void Configure(EntityTypeBuilder<AnimalRequest> entity)
        {
            entity.HasKey(e => new { e.IndCounter, e.FpsYear }).HasName("pk_tblanimalreq");

            entity.ToTable("tblanimalreq", "fps");

            entity.Property(e => e.IndCounter)
                .ValueGeneratedOnAdd()
                .HasColumnName("indcounter");
            entity.Property(e => e.AnimalType)
                .HasMaxLength(50)
                .HasColumnName("animaltype");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.JobCode)
                .HasMaxLength(20)
                .HasColumnName("jobcode");
            entity.Property(e => e.NumberOfAnimals).HasColumnName("numberofanimals");
            entity.Property(e => e.NumberOfDays).HasColumnName("numberofdays");
        }
    }
}
