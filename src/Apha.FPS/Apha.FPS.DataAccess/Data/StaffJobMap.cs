using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class StaffJobMap : IEntityTypeConfiguration<StaffJob>
    {
        private readonly IFpsRequestContext _fPSYearContext;

        public StaffJobMap(IFpsRequestContext fPSYearContext)
        {
            _fPSYearContext = fPSYearContext;
        }

        public void Configure(EntityTypeBuilder<StaffJob> entity)
        {
            entity.HasKey(e => new { e.StaffId, e.JobCode }).HasName("tblstaffjob_pk__tblstaffjob__30392ede");

            entity.ToTable("tblstaffjob", "fps");

            entity.Property(e => e.StaffId)
                .HasMaxLength(50)
                .HasColumnName("staffid");
            entity.Property(e => e.JobCode)
                .HasMaxLength(20)
                .HasColumnName("jobcode");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.PlannedHours).HasColumnName("plannedhours");
            entity.HasQueryFilter(e => e.FpsYear == _fPSYearContext.FpsYear);
        }
    }
}
