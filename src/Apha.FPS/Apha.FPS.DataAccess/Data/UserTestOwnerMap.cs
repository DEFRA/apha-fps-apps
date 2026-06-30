using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class UserTestOwnerMap : IEntityTypeConfiguration<UserTestOwner>
    {
        public void Configure(EntityTypeBuilder<UserTestOwner> entity)
        {
            entity.HasKey(e => new { e.TestOwner, e.UserId, e.FpsYear }).HasName("pk_tbluser_testowner");

            entity.ToTable("tbluser_testowner", "fps");

            entity.Property(e => e.TestOwner)
                .HasMaxLength(2)
                .HasColumnName("test_owner");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
        }
    }
}
