using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class UserCategoryMap : IEntityTypeConfiguration<UserCategory>
    {


        public void Configure(EntityTypeBuilder<UserCategory> entity)
        {
            entity.HasKey(e => new { e.UserId, e.Category }).HasName("tbluser_category_pk___6__10");

            entity.ToTable("tbluser_category", "fps");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Category)
                .HasMaxLength(20)
                .HasColumnName("category");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
        }
    }
}
