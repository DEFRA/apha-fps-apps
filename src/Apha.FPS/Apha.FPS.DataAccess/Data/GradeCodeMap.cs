using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class GradeCodeMap : IEntityTypeConfiguration<Grade>
    {
        public void Configure(EntityTypeBuilder<Grade> entity)
        {
            entity.HasKey(e => e.GradeCode);

            entity.ToTable("grade", "fps");

            entity.Property(e => e.GradeCode)
                .HasColumnName("gradecode");

            entity.Property(e => e.DescLong)
                .HasMaxLength(30)
                .HasColumnName("desc_long");
        }
    }
}
