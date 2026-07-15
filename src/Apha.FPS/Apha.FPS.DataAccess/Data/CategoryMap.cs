using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class CategoryMap : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> entity)
        {
            entity.HasKey(e => e.CategoryName).HasName("pk_tblcategory_1__10");

            entity.ToTable("tblcategory", "fps");

            entity.Property(e => e.CategoryName)
                .HasColumnType("citext")
                .HasColumnName("category");
        }
    }
}
