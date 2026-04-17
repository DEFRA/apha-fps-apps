using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class PactWorkGroupGradeViewMap : IEntityTypeConfiguration<PactWorkGroupGradeView>
    {
        private readonly IFpsRequestContext _fPSYearContext;

        public PactWorkGroupGradeViewMap(IFpsRequestContext fPSYearContext)
        {
            _fPSYearContext = fPSYearContext;
        }

        public void Configure(EntityTypeBuilder<PactWorkGroupGradeView> entity)
        {
            entity
                .HasNoKey()
                .ToView("vpactworkgroupgrade", "fps");

            entity.Property(e => e.AvSalary)
                .HasColumnType("money")
                .HasColumnName("avsalary");
            entity.Property(e => e.ChargeRateWg)
                .HasColumnType("money")
                .HasColumnName("chargerate_wg");
            entity.Property(e => e.DirectRateWg)
                .HasColumnType("money")
                .HasColumnName("directrate_wg");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.GradeCode)
                .HasColumnType("citext")
                .HasColumnName("gradecode");
            entity.Property(e => e.HrsChangedBy)
                .HasMaxLength(50)
                .HasColumnName("hrschangedby");
            entity.Property(e => e.NprWg)
                .HasColumnType("money")
                .HasColumnName("npr_wg");
            entity.Property(e => e.OhrWg)
                .HasColumnType("money")
                .HasColumnName("ohr_wg");
            entity.Property(e => e.PayRateWg)
                .HasColumnType("money")
                .HasColumnName("payrate_wg");
            entity.Property(e => e.ProfitCentreGrade)
                .HasColumnType("citext")
                .HasColumnName("profitcentregrade");
            entity.Property(e => e.WgGrade)
                .HasColumnType("citext")
                .HasColumnName("wg_grade");
            entity.Property(e => e.WorkGroup)
                .HasColumnType("citext")
                .HasColumnName("workgroup");
            entity.HasQueryFilter(e => e.FpsYear == _fPSYearContext.FpsYear);
        }
    }
}
