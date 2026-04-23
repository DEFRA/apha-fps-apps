using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class TimeCostCalcsViewMap : IEntityTypeConfiguration<TimeCostCalcsView>
    {
        private readonly IFpsRequestContext _fPSYearContext;

        public TimeCostCalcsViewMap(IFpsRequestContext fPSYearContext)
        {
            _fPSYearContext = fPSYearContext;
        }

        public void Configure(EntityTypeBuilder<TimeCostCalcsView> entity)
        {
            entity.HasNoKey().ToView("vtimecostcalcs", "fps");

            entity.Property(e => e.WorkGroup).HasColumnName("workgroup");
            entity.Property(e => e.JobCode).HasColumnName("jobcode");
            entity.Property(e => e.Project).HasColumnName("project");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.StaffId).HasColumnName("staffid");
            entity.Property(e => e.GradeCode).HasColumnName("gradecode");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.ChargeRate)
                .HasColumnType("money")
                .HasColumnName("chargerate");
            entity.Property(e => e.Class).HasColumnName("class");
            entity.Property(e => e.Time).HasColumnName("time");
            entity.Property(e => e.Cost).HasColumnName("cost");
            entity.Property(e => e.Division).HasColumnName("division");
            entity.Property(e => e.JobCodeOld).HasColumnName("jobcodeold");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");

            entity.HasQueryFilter(e => e.FpsYear == _fPSYearContext.FpsYear);
        }
    }
}
