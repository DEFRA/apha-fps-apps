using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class AnimalRequestViewMap : IEntityTypeConfiguration<AnimalRequestView>
    {
        private readonly IFpsRequestContext _fPSYearContext;

        public AnimalRequestViewMap(IFpsRequestContext fPSYearContext)
        {
            _fPSYearContext = fPSYearContext;
        }

        public void Configure(EntityTypeBuilder<AnimalRequestView> entity)
        {
            entity
                .HasNoKey()
                .ToView("vtblanimalreq", "fps");

            entity.Property(e => e.AnimalType)
                .HasColumnType("citext")
                .HasColumnName("animaltype");
            entity.Property(e => e.Dt2Username)
                .HasMaxLength(50)
                .UseCollation("latin1_general_ci_as")
                .HasColumnName("dt2username");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.IndCounter).HasColumnName("indcounter");
            entity.Property(e => e.JobCode)
                .HasColumnType("citext")
                .HasColumnName("jobcode");
            entity.Property(e => e.NumberOfAnimals).HasColumnName("numberofanimals");
            entity.Property(e => e.NumberOfDays).HasColumnName("numberofdays");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.UserEmail)
                .HasMaxLength(255)
                .UseCollation("latin1_general_ci_as")
                .HasColumnName("useremail");
            entity.HasQueryFilter(e => e.FpsYear == _fPSYearContext.FpsYear);
        }
    }
}
