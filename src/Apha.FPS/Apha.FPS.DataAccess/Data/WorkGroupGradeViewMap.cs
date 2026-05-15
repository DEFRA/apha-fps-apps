using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{

    public class WorkGroupGradeViewMap : IEntityTypeConfiguration<WorkGroupGradeView>
    {
        public void Configure(EntityTypeBuilder<WorkGroupGradeView> builder)
        {
            builder.HasNoKey();
            builder.ToView("vworkgroupgrade", "fps");

            builder.Property(e => e.WgGrade).HasColumnName("wggrade");
            builder.Property(e => e.ProfitCentreGrade).HasColumnName("profitcentregrade");
            builder.Property(e => e.GradeCode).HasColumnName("gradecode");
            builder.Property(e => e.WorkGroup).HasColumnName("workgroup");
            builder.Property(e => e.ChargeRateWg).HasColumnName("chargeratewg");
            builder.Property(e => e.DirectRateWg).HasColumnName("directratewg");
            builder.Property(e => e.PayRateWg).HasColumnName("payratewg");
            builder.Property(e => e.NprWg).HasColumnName("nprwg");
            builder.Property(e => e.OhrWg).HasColumnName("ohrwg");
            builder.Property(e => e.AvSalary).HasColumnName("avsalary");
            builder.Property(e => e.HrsChangedBy).HasColumnName("hrschangedby");
            builder.Property(e => e.FpsYear).HasColumnName("fpsyear");
            builder.Property(e => e.UserId).HasColumnName("user_id");
            builder.Property(e => e.Dt2Username).HasColumnName("dt2username");
            builder.Property(e => e.UserEmail).HasColumnName("useremail");
        }
    }
}