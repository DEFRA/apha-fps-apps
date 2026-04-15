using Apha.FPS.Core.Enities;
using Apha.FPS.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class WorkgroupGradeGeneralViewMap : IEntityTypeConfiguration<WorkgroupGradeGeneralView>
    {
        private readonly IFpsRequestContext _fPSYearContext;

        public WorkgroupGradeGeneralViewMap(IFpsRequestContext fPSYearContext)
        {
            _fPSYearContext = fPSYearContext;
        }

        public void Configure(EntityTypeBuilder<WorkgroupGradeGeneralView> entity)
        {
            entity
                .HasNoKey()
                .ToView("vworkgroupgrade_general", "fps");

            entity.Property(e => e.GradeCode)
                .HasMaxLength(50)
                .UseCollation("latin1_general_ci_as")
                .HasColumnName("gradecode");
            entity.Property(e => e.ProfitCentreGrade)
                .HasMaxLength(20)
                .HasColumnName("profitcentregrade");
            entity.Property(e => e.WgGrade)
                .HasMaxLength(50)
                .HasColumnName("wggrade");
            entity.Property(e => e.WorkGroup)
                .HasMaxLength(50)
                .HasColumnName("workgroup");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.HasQueryFilter(e => e.FpsYear == _fPSYearContext.FpsYear);
        }
    }
}
