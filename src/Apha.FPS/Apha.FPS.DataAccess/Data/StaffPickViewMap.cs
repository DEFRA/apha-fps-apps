using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class StaffPickViewMap : IEntityTypeConfiguration<StaffPickView>
    {


        public void Configure(EntityTypeBuilder<StaffPickView> entity)
        {
            entity
                .HasNoKey()
                .ToView("vtblstaff_pick", "fps");

            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.StaffId)
                .HasColumnType("citext")
                .HasColumnName("staffid");
            entity.Property(e => e.WorkgroupGrade)
                .HasColumnType("citext")
                .HasColumnName("workgroupgrade");
        }
    }
}
