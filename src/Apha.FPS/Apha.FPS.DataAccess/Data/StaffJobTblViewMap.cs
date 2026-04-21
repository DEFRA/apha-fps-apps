using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class StaffJobTblViewMap : IEntityTypeConfiguration<StaffJobTblView>
    {


        public void Configure(EntityTypeBuilder<StaffJobTblView> entity)
        {
            entity
                .HasNoKey()
                .ToView("vtblstaffjob", "fps");

            entity.Property(e => e.Dt2UserName)
                .HasMaxLength(50)
                .UseCollation("latin1_general_ci_as")
                .HasColumnName("dt2username");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.JobCode)
                .HasColumnType("citext")
                .HasColumnName("jobcode");
            entity.Property(e => e.PlannedHours).HasColumnName("plannedhours");
            entity.Property(e => e.StaffId)
                .HasColumnType("citext")
                .HasColumnName("staffid");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.UserEmail)
                .HasMaxLength(255)
                .UseCollation("latin1_general_ci_as")
                .HasColumnName("useremail");
        }
    }
}
