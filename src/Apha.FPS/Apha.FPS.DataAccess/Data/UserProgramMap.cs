using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class UserProgramMap : IEntityTypeConfiguration<UserProgram>
    {
        private readonly IFpsRequestContext _fPSYearContext;

        public UserProgramMap(IFpsRequestContext fPSYearContext)
        {
            _fPSYearContext = fPSYearContext;
        }

        public void Configure(EntityTypeBuilder<UserProgram> entity)
        {
            entity.HasKey(e => new { e.ProgramNo, e.UserID }).HasName("tbluser_program_pk__tbluser_program__26afc4a4");

            entity.ToTable("tbluser_program", "fps");

            entity.HasIndex(e => e.ProgramNo, "dbo_tbluser_program_xif84tbluser_program");

            entity.Property(e => e.ProgramNo)
                .HasMaxLength(10)
                .HasColumnName("programno");
            entity.Property(e => e.UserID).HasColumnName("user_id");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.HasQueryFilter(e => e.FpsYear == _fPSYearContext.FpsYear);
        }
    }
}
