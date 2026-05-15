using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{

    public class ProfitCentreGradeViewMap : IEntityTypeConfiguration<ProfitCentreGradeView>
    {
        public void Configure(EntityTypeBuilder<ProfitCentreGradeView> builder)
        {
            builder.HasNoKey();
            builder.ToView("vprofitcentregrade", "fps");

            builder.Property(e => e.PcGrade).HasColumnName("pcgrade");
            builder.Property(e => e.DivisionGrade).HasColumnName("divisiongrade");
            builder.Property(e => e.GradeCode).HasColumnName("gradecode");
            builder.Property(e => e.ProfitCentre).HasColumnName("profitcentre");
            builder.Property(e => e.ChargeRate).HasColumnName("chargerate");
            builder.Property(e => e.DirectRate).HasColumnName("directrate");
            builder.Property(e => e.PayRate).HasColumnName("payrate");
            builder.Property(e => e.Npr).HasColumnName("npr");
            builder.Property(e => e.Ohr).HasColumnName("ohr");
            builder.Property(e => e.HrsAvailable).HasColumnName("hrsavailable");
            builder.Property(e => e.OldChargeRate).HasColumnName("oldchargerate");
            builder.Property(e => e.DefraChargeRate).HasColumnName("defrachargerate");
            builder.Property(e => e.FpsYear).HasColumnName("fpsyear");
            builder.Property(e => e.UserId).HasColumnName("user_id");
            builder.Property(e => e.Dt2Username).HasColumnName("dt2username");
            builder.Property(e => e.UserEmail).HasColumnName("useremail");
        }
    }
}