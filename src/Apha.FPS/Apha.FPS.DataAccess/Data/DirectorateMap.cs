using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class DirectorateMap : IEntityTypeConfiguration<Directorate>
    {
        public void Configure(EntityTypeBuilder<Directorate> entity)
        {
            entity.HasKey(e => e.DirectorateName).HasName("pk_tbldirectorate");

            entity.ToTable("tbldirectorate", "fps");

            entity.Property(e => e.DirectorateName)
                .HasMaxLength(100)
                .HasColumnName("directorate");
        }
    }
}
