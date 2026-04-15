using Apha.FPS.Core.Enities;
using Apha.FPS.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class StaffActiveViewMap : IEntityTypeConfiguration<StaffActiveView>
    {
        private readonly IFpsRequestContext _fPSYearContext;

        public StaffActiveViewMap(IFpsRequestContext fPSYearContext)
        {
            _fPSYearContext = fPSYearContext;
        }

        public void Configure(EntityTypeBuilder<StaffActiveView> entity)
        {
            entity
                .HasNoKey()
                .ToView("vtblstaffactive", "fps");

            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.StaffID)
                .HasMaxLength(50)
                .HasColumnName("staffid");
            entity.Property(e => e.WorkgroupGrade)
                .HasMaxLength(50)
                .HasColumnName("workgroupgrade");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.HasQueryFilter(e => e.FpsYear == _fPSYearContext.FpsYear);
        }
    }
}
