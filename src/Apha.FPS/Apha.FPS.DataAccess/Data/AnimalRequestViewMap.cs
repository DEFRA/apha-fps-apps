using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class AnimalRequestViewMap : IEntityTypeConfiguration<AnimalRequestView>
    {


        public void Configure(EntityTypeBuilder<AnimalRequestView> entity)
        {
            entity
                .HasNoKey()
                .ToView("vtblanimalreq", "fps");

            entity.Property(e => e.AnimalType)
                .HasMaxLength(50)
                .HasColumnName("animaltype");
            entity.Property(e => e.Dt2Username)
                .HasMaxLength(50)
                .HasColumnName("dt2username");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.IndCounter).HasColumnName("indcounter");
            entity.Property(e => e.JobCode)
                .HasMaxLength(20)
                .HasColumnName("jobcode");
            entity.Property(e => e.NumberOfAnimals).HasColumnName("numberofanimals");
            entity.Property(e => e.NumberOfDays).HasColumnName("numberofdays");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.UserEmail)
                .HasMaxLength(255)
                .HasColumnName("useremail");
        }
    }
}
