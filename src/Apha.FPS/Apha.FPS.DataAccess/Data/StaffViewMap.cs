using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class StaffViewMap : IEntityTypeConfiguration<StaffView>
    {


        public void Configure(EntityTypeBuilder<StaffView> entity)
        {
            entity
                .HasNoKey()
                .ToView("vtblstaff", "fps");

            entity.Property(e => e.Dt2Username)
                .HasMaxLength(50)
                .UseCollation("latin1_general_ci_as")
                .HasColumnName("dt2username");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.HrsAvail).HasColumnName("hrsavail");
            entity.Property(e => e.HrsPaid).HasColumnName("hrspaid");
            entity.Property(e => e.Leave).HasColumnName("leave");
            entity.Property(e => e.MakeAvailable).HasColumnName("makeavailable");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.PersonClass)
                .HasMaxLength(10)
                .HasColumnName("personclass");
            entity.Property(e => e.PersonStatus)
                .HasMaxLength(10)
                .HasColumnName("personstatus");
            entity.Property(e => e.SickSpecial).HasColumnName("sickspecial");
            entity.Property(e => e.StaffId)
                .HasColumnType("citext")
                .HasColumnName("staffid");
            entity.Property(e => e.Title)
                .HasMaxLength(4)
                .HasColumnName("title");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.UserEmail)
                .HasMaxLength(255)
                .UseCollation("latin1_general_ci_as")
                .HasColumnName("useremail");
            entity.Property(e => e.WorkgroupGrade)
                .HasColumnType("citext")
                .HasColumnName("workgroupgrade");
        }
    }
}
