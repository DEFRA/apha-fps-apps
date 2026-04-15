using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class StaffGeneralViewMap : IEntityTypeConfiguration<StaffGeneralView>
    {
        private readonly IFpsRequestContext _fPSYearContext;

        public StaffGeneralViewMap(IFpsRequestContext fPSYearContext)
        {
            _fPSYearContext = fPSYearContext;
        }

        public void Configure(EntityTypeBuilder<StaffGeneralView> entity)
        {
            entity
                .HasNoKey()
                .ToView("vtblstaff_general", "fps");

            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.StaffId)
                .HasColumnType("citext")
                .HasColumnName("staffid");
            entity.Property(e => e.WorkGroupGrade)
                .HasColumnType("citext")
                .HasColumnName("workgroupgrade");
            entity.HasQueryFilter(e => e.FpsYear == _fPSYearContext.FpsYear);
        }
    }
}
