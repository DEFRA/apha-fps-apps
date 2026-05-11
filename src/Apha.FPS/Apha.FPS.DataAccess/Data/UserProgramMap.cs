using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class UserProgramMap : IEntityTypeConfiguration<UserProgram>
    {


        public void Configure(EntityTypeBuilder<UserProgram> entity)
        {
            entity.HasKey(e => new { e.ProgramNo, e.UserID, e.FpsYear }).HasName("pk_tbluser_program");

            entity.ToTable("tbluser_program", "fps");

            entity.HasIndex(e => e.ProgramNo, "xif84tbluser_program");

            entity.Property(e => e.ProgramNo)
                .HasMaxLength(10)
                .HasColumnName("programno");
            entity.Property(e => e.UserID).HasColumnName("user_id");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
        }
    }
}
