using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class ProgramMap : IEntityTypeConfiguration<Program>
    {
        private readonly IFpsRequestContext _fPSYearContext;

        public ProgramMap(IFpsRequestContext fPSYearContext)
        {
            _fPSYearContext = fPSYearContext;
        }

        public void Configure(EntityTypeBuilder<Program> entity)
        {
            entity.HasKey(e => e.ProgramNo).HasName("tlkpprogram_pk__tlkpprogram__2180fb33");

            entity.ToTable("tlkpprogram", "fps");

            entity.HasIndex(e => e.Minim, "dbo_tlkpprogram_tlkpprogram_minim");

            entity.Property(e => e.ProgramNo)
                .HasMaxLength(10)
                .HasColumnName("programno");
            entity.Property(e => e.Customer)
                .HasMaxLength(50)
                .HasColumnName("customer");
            entity.Property(e => e.Directorate)
                .HasMaxLength(15)
                .HasColumnName("directorate");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.Manager)
                .HasMaxLength(50)
                .HasColumnName("manager");
            entity.Property(e => e.Minim)
                .HasMaxLength(7)
                .HasColumnName("minim");
            entity.Property(e => e.ProgramName)
                .HasMaxLength(80)
                .HasColumnName("programname");
            entity.Property(e => e.SectorName)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Charge'::character varying")
                .HasColumnName("sector_name");
            entity.Property(e => e.Target)
                .HasDefaultValueSql("0")
                .HasColumnType("money")
                .HasColumnName("target");
            entity.HasQueryFilter(e => e.FpsYear == _fPSYearContext.FpsYear);
        }
    }
}
