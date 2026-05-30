using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class UserProfitcentreMap : IEntityTypeConfiguration<UserProfitcentre>
    {


        public void Configure(EntityTypeBuilder<UserProfitcentre> entity)
        {
            entity.HasKey(e => new { e.ProfitCentre, e.UserId, e.FpsYear }).HasName("pk_tbluser_profitcentre");

            entity.ToTable("tbluser_profitcentre", "fps");

            entity.HasIndex(e => e.UserId, "xif89tbluser_profitcentre");

            entity.HasIndex(e => e.ProfitCentre, "xif90tbluser_profitcentre");

            entity.Property(e => e.ProfitCentre)
                .HasMaxLength(50)
                .HasColumnName("profitcentre");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
        }
    }
}
