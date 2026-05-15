using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class ProjectGroupViewMap : IEntityTypeConfiguration<ProjectGroupView>
    {
        public void Configure(EntityTypeBuilder<ProjectGroupView> entity)
        {
            entity
                .HasNoKey()
                .ToView("vtlkpprojectgroup", "fps");

            entity.Property(e => e.ProjectGroupName)
                .HasMaxLength(50)
                .HasColumnName("projectgroup");

            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.Property(e => e.UserEmail)
                .HasMaxLength(255)
                .UseCollation("latin1_general_ci_as")
                .HasColumnName("useremail");
        }
    }
}
