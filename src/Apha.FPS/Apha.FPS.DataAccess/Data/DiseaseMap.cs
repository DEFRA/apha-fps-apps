using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class DiseaseMap : IEntityTypeConfiguration<Disease>
    {
        public void Configure(EntityTypeBuilder<Disease> entity)
        {
            entity.HasKey(e => e.DiseaseName).HasName("pk___4__10");

            entity.ToTable("tbldisease", "fps");

            entity.Property(e => e.DiseaseName)
                .HasMaxLength(50)
                .HasColumnName("disease");
        }
    }
}
