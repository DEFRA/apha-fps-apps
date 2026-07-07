using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class UserProjectGroupMap : IEntityTypeConfiguration<UserProjectGroup>
    {
        public void Configure(EntityTypeBuilder<UserProjectGroup> entity)
        {
            entity.HasKey(e => new { e.ProjectGroup, e.UserId, e.FpsYear }).HasName("pk_tbluser_projectgroup");

            entity.ToTable("tbluser_projectgroup", "fps");

            entity.Property(e => e.ProjectGroup)
                .HasMaxLength(50)
                .HasColumnName("projectgroup");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
        }
    }
}
