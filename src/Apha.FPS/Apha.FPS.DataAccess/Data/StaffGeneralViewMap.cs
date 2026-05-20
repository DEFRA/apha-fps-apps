using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class StaffGeneralViewMap : IEntityTypeConfiguration<StaffGeneralView>
    {


        public void Configure(EntityTypeBuilder<StaffGeneralView> entity)
        {
            entity
                .HasNoKey()
                .ToView("vtblstaff_general", "fps");

            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.StaffId)
                .HasMaxLength(50)
                .HasColumnName("staffid");
            entity.Property(e => e.WorkGroupGrade)
                .HasMaxLength(50)
                .HasColumnName("workgroupgrade");
        }
    }
}
