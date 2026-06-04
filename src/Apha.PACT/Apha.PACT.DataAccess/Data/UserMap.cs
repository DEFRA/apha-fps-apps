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
                .HasColumnName("comments");  // SWAPPED - this was the bug

            entity.Property(e => e.Comments)
                .HasColumnName("username");  // SWAPPED - this was the bug
        }
    }
}
