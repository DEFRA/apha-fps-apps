using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    /// <summary>
    /// EF Core entity type configuration for the Grade entity.
    /// </summary>
    public class GradeMap : IEntityTypeConfiguration<Grade>
    {
        public void Configure(EntityTypeBuilder<Grade> entity)
        {
            entity.HasKey(e => e.GradeCode).HasName("pk_grade");

            entity.ToTable("grade", "fps");

            entity.Property(e => e.GradeCode)
                .HasMaxLength(50)
                .HasColumnName("gradecode");

            entity.Property(e => e.FpsYear)
                .HasColumnName("fpsyear");
        }
    }
}
