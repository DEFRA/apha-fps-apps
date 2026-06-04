using Apha.PACT.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PACT.DataAccess.Data
{
    public class UserMap : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> entity)
        {
            entity.HasKey(e => e.UserId).HasName("pk__tblusers__1367e606");

            entity.ToTable("tblusers", "fps");

            entity.HasIndex(e => e.UserName , "username").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.AgencyId ).HasColumnName("agencyid");
            entity.Property(e => e.Comments)
                .HasMaxLength(255)
                .HasColumnName("comments");
            entity.Property(e => e.Dt2UserName)
                .HasMaxLength(50)
                .HasColumnName("dt2username");
            entity.Property(e => e.FrmWarning)
            .HasDefaultValue(false)
            .HasColumnName("frmwarning");
            entity.Property(e => e.UserEmail)
                .HasMaxLength(255)
                .HasColumnName("useremail");
            entity.Property(e => e.UserName)
                .HasMaxLength(50)
                .HasColumnName("username");
        }
    }
}
