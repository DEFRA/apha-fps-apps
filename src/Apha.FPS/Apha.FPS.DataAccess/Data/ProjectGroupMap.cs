using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class ProjectGroupMap : IEntityTypeConfiguration<ProjectGroup>
    {
        public void Configure(EntityTypeBuilder<ProjectGroup> entity)
        {
            entity.HasKey(e => e.ProjectGroupName).HasName("tlkpprojectgroup_pk_tlkpprojectgroup");

            entity.ToTable("tlkpprojectgroup", "fps");

            entity.Property(e => e.ProjectGroupName)
                .HasMaxLength(50)
                .HasColumnName("projectgroup");
        }
    }
}
