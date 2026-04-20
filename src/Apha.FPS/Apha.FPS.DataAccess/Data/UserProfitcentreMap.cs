using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class UserProfitcentreMap : IEntityTypeConfiguration<UserProfitcentre>
    {
        private readonly IFpsRequestContext _fPSYearContext;

        public UserProfitcentreMap(IFpsRequestContext fPSYearContext)
        {
            _fPSYearContext = fPSYearContext;
        }

        public void Configure(EntityTypeBuilder<UserProfitcentre> entity)
        {
            entity.HasKey(e => new { e.ProfitCentre, e.UserId }).HasName("tbluser_profitcentre_pk__tbluser_profitce__77bfcb91");

            entity.ToTable("tbluser_profitcentre", "fps");

            entity.HasIndex(e => e.UserId, "dbo_tbluser_profitcentre_xif89tbluser_profitcentre");

            entity.HasIndex(e => e.ProfitCentre, "dbo_tbluser_profitcentre_xif90tbluser_profitcentre");

            entity.Property(e => e.ProfitCentre)
                .HasMaxLength(50)
                .HasColumnName("profitcentre");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.HasQueryFilter(e => e.FpsYear == _fPSYearContext.FpsYear);
        }
    }
}
