using Apha.PACT.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PACT.DataAccess.Data
{
    public class UserMap : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> entity)
        {
            entity.HasKey(e => e.UserName);

            entity.ToTable("tblusers", "fps");
            entity.HasKey(e => e.UserName);
            entity.Property(e => e.UserName)
                .HasMaxLength(100)
                .HasColumnName("username");

            entity.Property(e => e.Comments)
                .HasColumnName("comments");

            entity.HasMany(e => e.Logs)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .HasPrincipalKey(e => e.UserName)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
