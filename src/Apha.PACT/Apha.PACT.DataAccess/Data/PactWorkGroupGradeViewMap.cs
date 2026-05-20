using Apha.PACT.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PACT.DataAccess.Data
{
    public class PactWorkGroupGradeViewMap : IEntityTypeConfiguration<PactWorkGroupGradeView>
    {
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
                .HasMaxLength(10)
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
                .HasMaxLength(20)
                .HasColumnName("profitcentregrade");
            entity.Property(e => e.WgGrade)
                .HasMaxLength(50)
                .HasColumnName("wg_grade");
            entity.Property(e => e.WorkGroup)
                .HasMaxLength(50)
                .HasColumnName("workgroup");
        }
    }
}
