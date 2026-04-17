using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class UserMap : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> entity)
        {
            entity.HasKey(e => e.UserId).HasName("tblusers_pk__tblusers__1367e606");

            entity.ToTable("tblusers", "fps");

            entity.HasIndex(e => e.Username, "dbo_tblusers_username")
                .IsUnique()
                .UseCollation(new[] { "latin1_general_ci_as" });

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.AgencyId).HasColumnName("agencyid");
            entity.Property(e => e.Comments)
                .HasMaxLength(255)
                .UseCollation("latin1_general_ci_as")
                .HasColumnName("comments");
            entity.Property(e => e.Dt2Username)
                .HasMaxLength(50)
                .UseCollation("latin1_general_ci_as")
                .HasColumnName("dt2username");
            entity.Property(e => e.FrmWarning).HasColumnName("frmwarning");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .UseCollation("latin1_general_ci_as")
                .HasColumnName("username");
            entity.Property(e => e.UserEmail)
                .HasMaxLength(255)
                .UseCollation("latin1_general_ci_as")
                .HasColumnName("useremail");
        }
    }
}
